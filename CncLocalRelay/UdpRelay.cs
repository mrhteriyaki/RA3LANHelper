using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace CncLocalRelay
{
    class UdpRelay
    {
        public static IPAddress NatNegRealServer;

        public static bool run_relay = true;
        int natneg_port = 27901; //NAT Negotiation Server Port
        bool _UPNP = false;
        List<int> OpenedPortsUPNP = new List<int>();

        UdpClient localNatNegUdpClient;
        Thread inThread;

        List<ConnectionSession> sessionList;
        List<ConnectionSession> P2PClients;
        int _StartPortOffset = 50000;

        public UdpRelay(int StartPortOffset, bool UPNP)
        {
            if (NatNegRealServer == null)
            {
                throw new Exception("Must set NatNegRealServer");
            }
            localNatNegUdpClient = new UdpClient(natneg_port);
            _UPNP = UPNP;
            sessionList = new List<ConnectionSession>();
            P2PClients = new List<ConnectionSession>();
            _StartPortOffset = StartPortOffset;
        }

        public void Relay()
        {
            IPEndPoint targetEndpoint = new IPEndPoint(NatNegRealServer, natneg_port);
            IPEndPoint incomingEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (run_relay)
            {
                //Incoming data from local RA3 client destined for NAT NEG Server.
                byte[] receivedDataLocal;
                try
                {
                    receivedDataLocal = localNatNegUdpClient.Receive(ref incomingEndPoint);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Error:" + ex);
                    continue;
                }

                //Setup ConnectionSession for each unique source port identified from local ra3 client, remap port number to user set range.
                bool new_session = true;
                int counter = 0;
                foreach (ConnectionSession CSS in sessionList)
                {
                    if (CSS.client.Equals(incomingEndPoint))
                    {
                        new_session = false;
                        break;
                    }
                    counter++;
                }
                //ConnectionSession provides relay for each re-mapped port for p2p clients, port numbers will be related by control server to other clients.
                if (new_session)
                {
                    int localport = counter + _StartPortOffset;
                    sessionList.Add(new ConnectionSession(incomingEndPoint.Address, incomingEndPoint.Port, localport));
                    Trace.WriteLine($"New outbound connection {sessionList[counter].udpClient.Client.LocalEndPoint} {targetEndpoint}");
                    if (_UPNP)
                    {
                        OpenedPortsUPNP.Add(localport);
                        UPNPControl.OpenPortAsync(localport);
                    }
                    Thread sessionThread = new Thread(() => sessionRelay(counter));
                    sessionThread.Start();
                }
                sessionList[counter].udpClient.Send(receivedDataLocal, receivedDataLocal.Length, targetEndpoint);
            }
        }

        void sessionRelay(int session_index)
        {
            //Process inbound packets.
            while (run_relay)
            {
                try
                {
                    byte[] receivedDataRemote;
                    try
                    {
                        receivedDataRemote = sessionList[session_index].udpClient.Receive(ref sessionList[session_index].remoteServer);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("Error:" + ex);
                        continue;
                    }

                    if (sessionList[session_index].IsNatNeg())
                    {
                        //Response from real NAT NEG server, relay to local ra3 client using natneg source port.

                        //Direct Peer address replacement.
                        var pubip = PublicIP.Get();
                        byte[] findByte = [pubip[0], pubip[1], pubip[2], pubip[3]];
                        //for each neighbor, replace with their local ip and port number start.
                        
                        foreach (var nb in LocalNeighbours.GetList())
                        {
                            byte[] replaceByte = nb.Address.GetAddressBytes();
                            ReplacePattern(receivedDataRemote, findByte, replaceByte, nb.StartPort);
                        }

                        localNatNegUdpClient.Send(receivedDataRemote, receivedDataRemote.Length, sessionList[session_index].client);
                    }
                    else
                    {
                        //P2P Traffic - give the internal loopback interface different ports per client for identification of return sender.
                        //Clients cannot share port numbers due to common 127.0.0.1 ip - split clients up by port for identifcation by local ra3.
                        P2PRelaySetup(sessionList[session_index].remoteServer, receivedDataRemote, sessionList[session_index].client, session_index);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Connection closed: " + sessionList[session_index].client + " Exception:" + ex.ToString());
                }
            }
        }

        void P2PRelaySetup(IPEndPoint P2PClientEndpoint, byte[] message, IPEndPoint LocalEndpoint, int session_index)
        {
            //Create new UDP Client to relay each P2P client to a seperate loopback port.
            bool newclient = true;
            int count = 0;
            foreach (ConnectionSession PClient in P2PClients)
            {
                if (PClient.client.Equals(P2PClientEndpoint))
                {
                    newclient = false;
                    break;
                }
                count++;
            }

            if (newclient)
            {
                P2PClients.Add(new ConnectionSession(P2PClientEndpoint.Address, P2PClientEndpoint.Port, 0));
                Trace.WriteLine("New peer connection: " + P2PClientEndpoint + " " + sessionList[session_index].udpClient.Client.LocalEndPoint);
                Thread P2PReplyThread = new Thread(() => P2PRelay(count, session_index));
                P2PReplyThread.Start();
            }

            //Relay Data to local endpoint.
            //Trace.WriteLine($"Relay P2P Message from {P2PClients[count].client}");
            P2PClients[count].udpClient.Send(message, message.Length, LocalEndpoint);
        }

        void P2PRelay(int P2PIndex, int session_index)
        {
            //Data sent from Local Client to p2p loopback ports, relay from original udp session relays.
            while (run_relay)
            {
                try
                {
                    byte[] receivedDataRemote;

                    try
                    {
                        receivedDataRemote = P2PClients[P2PIndex].udpClient.Receive(ref P2PClients[P2PIndex].remoteServer);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("Error:" + ex);
                        continue;
                    }

                    //Relay internal message by original port to client.
                    sessionList[session_index].udpClient.Send(receivedDataRemote, receivedDataRemote.Length, P2PClients[P2PIndex].client);
                    //Trace.WriteLine($"Relay P2P Message Back to {P2PClients[P2PIndex].client}");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("P2P Connection Closed: " + P2PClients[P2PIndex].client); //connection closed.
                    Trace.WriteLine(ex);
                }
            }
        }


        public void StopRelay()
        {
            try
            {
                foreach (int Portnumber in OpenedPortsUPNP)
                {
                    UPNPControl.ClosePortAsync(Portnumber);
                }
                OpenedPortsUPNP.Clear();
            }
            catch (Exception uex)
            {
                Debug.WriteLine("Error closing upnp: " + uex.Message);
            }


            localNatNegUdpClient.Close();
            foreach (ConnectionSession CSS in sessionList)
            {
                CSS.udpClient.Close();
            }
            foreach (ConnectionSession P2P in P2PClients)
            {
                P2P.udpClient.Close();
            }
            run_relay = false;


        }

        static void ReplacePattern(byte[] data, byte[] findPrefix4, byte[] replacePrefix4, int StartPortRange, int range = 50)
        {
            if (findPrefix4.Length != 4 || replacePrefix4.Length != 4)
            {
                throw new ArgumentException("findPrefix4 and replacePrefix4 must be exactly 4 bytes.");
            }

            ushort startPort = (ushort)StartPortRange;

            ushort endPort = (ushort)(startPort + range);

            for (int i = 0; i <= data.Length - 6; i++)
            {
                // Match first 4 bytes (the "IP"/prefix portion)
                bool match = true;
                for (int j = 0; j < 4; j++)
                {
                    if (data[i + j] != findPrefix4[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (!match) continue;

                // Read port (big-endian / network byte order)
                ushort port = (ushort)((data[i + 4] << 8) | data[i + 5]);

                // Only replace if port is in [startPort..startPort+range]
                if (port < startPort || port > endPort)
                {
                    continue;
                }

                // Replace the 4-byte prefix, keep the port bytes as-is
                Buffer.BlockCopy(replacePrefix4, 0, data, i, 4);

                i += 5; // skip past this match
            }
        }



    }
}
