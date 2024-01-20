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

        UdpClient localUdpClient;
        Thread inThread;

        List<ConnectionSession> sessionList;
        List<ConnectionSession> P2PClients;
        int _StartPortOffset = 50000;

        public UdpRelay(string targetIP, int Port, int StartPortOffset)
        {
            TargetIP = IPAddress.Parse(targetIP);
            _port = Port;
            localUdpClient = new UdpClient(_port);

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

        void Relay() // Local -> This Relay -> Remote IPs
        {
            IPEndPoint targetEndpoint = new IPEndPoint(TargetIP, _port); //CNC-ONLINE:27901
            IPEndPoint incomingEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (run_relay)
            {
                byte[] receivedDataLocal = localUdpClient.Receive(ref incomingEndPoint);

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
                    //Console.WriteLine("New session from " + incomingEndPoint);
                    sessionList.Add(new ConnectionSession(incomingEndPoint.Address, incomingEndPoint.Port, counter + _StartPortOffset));

                    Thread sessionThread = new Thread(() => sessionRelay(counter));
                    sessionThread.Start();
                }
                sessionList[counter].udpClient.Send(receivedDataLocal, receivedDataLocal.Length, targetEndpoint);
                //Console.WriteLine($" RLY {incomingEndPoint} -> {sessionList[counter].udpClient.Client.LocalEndPoint} -> {targetEndpoint} HASH:{receivedDataLocal.GetHashCode()}");

            }
        }

        void sessionRelay(int session_index) //Remote IPs -> This Relay:0 -> Local
        {
            while (run_relay)
            {

                byte[] receivedDataRemote = sessionList[session_index].udpClient.Receive(ref sessionList[session_index].remoteServer);

                //Origin check, if response from target server, use localUdpClient (27901) otherwise P2P Traffic.
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

                    //CheckData(receivedDataRemote, sessionList[session_index].client.Port); //Replace P2P Client IP:Port with Local IP.
                    //sessionList[session_index].udpClient.Send(receivedDataRemote, receivedDataRemote.Length, sessionList[session_index].client);
                    //Console.WriteLine($"PRLY {sessionList[session_index].remoteServer} -> {sessionList[session_index].udpClient.Client.LocalEndPoint} -> {sessionList[session_index].client} HASH:{receivedDataRemote.GetHashCode()}");
                }


            }
        }

        void P2PInternalRelay(IPEndPoint P2PClientEndpoint, byte[] message, IPEndPoint LocalEndpoint, int session_index)
        {
            //Create new UDP Client to relay each P2P client if not already.
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
                Console.WriteLine("New peer connected: " + P2PClientEndpoint);
                P2PClients.Add(new ConnectionSession(P2PClientEndpoint.Address, P2PClientEndpoint.Port, 0));
                Thread P2PReplyThread = new Thread(() => P2PRelay(count, session_index));
                P2PReplyThread.Start();
            }

            //Relay Data to local endpoint.
            //Console.WriteLine($"Relay P2P Message from {P2PClients[count].client}");
            P2PClients[count].udpClient.Send(message, message.Length, LocalEndpoint);


        }

        void P2PRelay(int P2PIndex, int session_index)
        {
            try
            {
                while (run_relay)
                {

                    byte[] receivedDataRemote = P2PClients[P2PIndex].udpClient.Receive(ref P2PClients[P2PIndex].remoteServer);
                    //Relay internal message by original port to client.
                    sessionList[session_index].udpClient.Send(receivedDataRemote, receivedDataRemote.Length, P2PClients[P2PIndex].client);
                    //Console.WriteLine($"Relay P2P Message Back to {P2PClients[P2PIndex].client}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); //connection closed.
            }

        }


        void StopRelay()
        {
            localUdpClient.Close();
            foreach (ConnectionSession CSS in sessionList)
            {
                CSS.udpClient.Close();
            }
            run_relay = false;
        }

        public void RunRelay()
        {
            inThread = new Thread(Relay);
            inThread.Start();

        }





    }
}
