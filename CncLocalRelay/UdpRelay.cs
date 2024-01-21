using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace CncLocalRelay
{
    class UdpRelay
    {
        bool run_relay = true;
        IPAddress TargetIP;
        int _port;
        
        bool _UPNP = false;
        List<int> OpenedPortsUPNP = new List<int>();

        UdpClient localUdpClient;
        Thread inThread;

        List<ConnectionSession> sessionList;
        List<ConnectionSession> P2PClients;
        int _StartPortOffset = 50000;

        public UdpRelay(string targetIP, int Port, int StartPortOffset, bool UPNP)
        {
            TargetIP = IPAddress.Parse(targetIP);
            _port = Port;
            localUdpClient = new UdpClient(_port);
            _UPNP = UPNP;
            sessionList = new List<ConnectionSession>();
            P2PClients = new List<ConnectionSession>();
            _StartPortOffset = StartPortOffset;
        }

        class ConnectionSession
        {
            public IPEndPoint client;
            public IPEndPoint remoteServer;
            public UdpClient udpClient;

            public ConnectionSession(IPAddress IP, int Port, int LocalPort)
            {
                client = new IPEndPoint(IP, Port);
                udpClient = new UdpClient(LocalPort);
                udpClient.Client.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
                udpClient.AllowNatTraversal(true);

                remoteServer = new IPEndPoint(IPAddress.Any, 0);
            }
        }

        void Relay()
        {
            IPEndPoint targetEndpoint = new IPEndPoint(TargetIP, _port);
            IPEndPoint incomingEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (run_relay)
            {
                byte[] receivedDataLocal;
                try
                {
                    receivedDataLocal = localUdpClient.Receive(ref incomingEndPoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error:" + ex);
                    continue;
                }


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

                if (new_session)
                {
                    int localport = counter + _StartPortOffset;
                    sessionList.Add(new ConnectionSession(incomingEndPoint.Address, incomingEndPoint.Port, localport));
                    Console.WriteLine("New outbound connection " + sessionList[counter].udpClient.Client.LocalEndPoint + " " + targetEndpoint);
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
                        Console.WriteLine("Error:" + ex);
                        continue;
                    }

                    //Origin check, if response from target server, use localUdpClient otherwise P2P Traffic.
                    if (sessionList[session_index].remoteServer.Address.Equals(TargetIP))
                    {
                        //Target NAT Server
                        localUdpClient.Send(receivedDataRemote, receivedDataRemote.Length, sessionList[session_index].client);
                        //Console.WriteLine($"SRLY {sessionList[session_index].remoteServer} -> {localUdpClient.Client.LocalEndPoint} -> {sessionList[session_index].client} HASH:{receivedDataRemote.GetHashCode()}");
                    }
                    else
                    {
                        //P2P Traffic - give the internal loopback interface different ports per client for identification of return sender.
                        P2PInternalRelay(sessionList[session_index].remoteServer, receivedDataRemote, sessionList[session_index].client, session_index);

                        //Console.WriteLine($"PRLY {sessionList[session_index].remoteServer} -> {sessionList[session_index].udpClient.Client.LocalEndPoint} -> {sessionList[session_index].client} HASH:{receivedDataRemote.GetHashCode()}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Connection closed: " + sessionList[session_index].client + " Exception:" + ex.ToString());
                }




            }
        }

        void P2PInternalRelay(IPEndPoint P2PClientEndpoint, byte[] message, IPEndPoint LocalEndpoint, int session_index)
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
                Console.WriteLine("New peer connection: " + P2PClientEndpoint + " " + sessionList[session_index].udpClient.Client.LocalEndPoint);
                Thread P2PReplyThread = new Thread(() => P2PRelay(count, session_index));
                P2PReplyThread.Start();
            }

            //Relay Data to local endpoint.
            //Console.WriteLine($"Relay P2P Message from {P2PClients[count].client}");
            P2PClients[count].udpClient.Send(message, message.Length, LocalEndpoint);


        }

        void P2PRelay(int P2PIndex, int session_index)
        {

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
                        Console.WriteLine("Error:" + ex);
                        continue;
                    }

                    //Relay internal message by original port to client.
                    sessionList[session_index].udpClient.Send(receivedDataRemote, receivedDataRemote.Length, P2PClients[P2PIndex].client);
                    //Console.WriteLine($"Relay P2P Message Back to {P2PClients[P2PIndex].client}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("P2P Connection Closed: " + P2PClients[P2PIndex].client); //connection closed.
                    Console.WriteLine(ex);
                }
            }

        }


        public void StopRelay()
        {
            localUdpClient.Close();
            foreach (ConnectionSession CSS in sessionList)
            {
                CSS.udpClient.Close();
            }
            foreach(ConnectionSession P2P in P2PClients)
            {
                P2P.udpClient.Close();
            }
            run_relay = false;

            foreach(int Portnumber in OpenedPortsUPNP)
            {
                UPNPControl.ClosePortAsync(Portnumber);
            }
            OpenedPortsUPNP.Clear();

        }

        public void RunRelay()
        {
            inThread = new Thread(Relay);
            inThread.Start();
        }





    }
}
