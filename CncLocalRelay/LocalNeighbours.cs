using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace CncLocalRelay
{
    public class LocalNeighbours
    {
        public IPAddress Address;
        public int StartPort;
        public List<GameConnSession> remoteSessions = new();

        static List<LocalNeighbours> neighboursList = new();
        static List<GameConnSession> localSessions = new();

        private static readonly object _nLock = new();
        private static readonly object _sLock = new();
        private static readonly object _sendLock = new();

        static int localStartPort;
        static UdpClient nbUdpClient;
        static bool initComplete = false;
        static bool shutdown = false;

        public static readonly int peerDetectionPort = 48632;

        public static void InitPort()
        {
            while (!initComplete && !shutdown)
            {
                try
                {
                    nbUdpClient = new UdpClient(peerDetectionPort);
                    initComplete = true;
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    Thread.Sleep(150);
                }
            }
        }

        public static bool CheckInit()
        {
            return initComplete;
        }

        public static void RunDiscovery(int startPort)
        {
            InitPort();

            localStartPort = startPort;

            neighboursList.Clear();
            localSessions.Clear();

            Thread runSend = new Thread(() => SendAdvertLoop());
            runSend.Name = "Neighbours SendAdvert Thread";
            runSend.Start();

            // Allow reuse if multiple apps / restarts
            nbUdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            nbUdpClient.EnableBroadcast = true;

            while (UdpRelay.run_relay)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data;
                try
                {
                    data = nbUdpClient.Receive(ref remoteEP); // BLOCKS until a packet arrives
                }
                catch(System.Net.Sockets.SocketException socketex)
                {
                    continue;
                }
                

                bool skipLocal = false;
                foreach (var localIP in GetLocalIPAddresses())
                {
                    if (remoteEP.Address.Equals(localIP))
                    {
                        skipLocal = true;
                        break;
                    }
                }
                if (remoteEP.Address.Equals(IPAddress.Loopback))
                {
                    skipLocal = true;
                }
                if (remoteEP.Address.Equals(IPAddress.IPv6Loopback))
                {
                    skipLocal = true;
                }
                if (skipLocal)
                {
                    continue;
                }

                string dataString = Encoding.ASCII.GetString(data, 0, data.Length);

                if (!dataString.Contains(","))
                {
                    Trace.WriteLine($"Packet with invalid message from {remoteEP.Address.ToString()}");
                    continue;
                }
                var splitData = dataString.Split(",");
                if (!splitData[0].Equals(splitData[1]))
                {
                    Trace.WriteLine($"Packet with corrupt message from {remoteEP.Address.ToString()}");
                    continue;
                }
                try
                {
                    int startPortRemote = int.Parse(splitData[0]);
                    bool nExists = false;
                    lock (_nLock)
                    {
                        foreach (var nb in neighboursList)
                        {
                            if (nb.Address.Equals(remoteEP.Address))
                            {
                                //Port changes.
                                if(nb.StartPort != startPortRemote)
                                {
                                    nb.StartPort = startPortRemote; //Port has changed on remote peer.
                                    Trace.WriteLine($"Remote start port has changed on remote peer: {nb.Address}:{nb.StartPort}");
                                }

                                //Game sessions.
                                nb.remoteSessions.Clear();
                                foreach (string gdata in splitData.Skip(2))
                                {
                                    var gcsdata = gdata.Split(":");
                                    GameConnSession gcs = new GameConnSession();
                                    gcs.sessionId = int.Parse(gcsdata[0]);
                                    gcs.connectionId = int.Parse(gcsdata[1]);
                                    gcs.localport = int.Parse(gcsdata[2]);
                                    nb.remoteSessions.Add(gcs);
                                }
                                nExists = true;
                                break;
                            }
                        }
                    }
                    if (!nExists)
                    {
                        Trace.WriteLine($"Neighbour Detected: {remoteEP.Address.ToString()}:{startPortRemote}");
                        lock (_nLock)
                        {
                            neighboursList.Add(new LocalNeighbours()
                            {
                                Address = remoteEP.Address,
                                StartPort = startPortRemote
                            });
                        }
                    }
                }
                catch(Exception ex)
                {
                    Trace.WriteLine($"Neighbours Thread Failure: {ex.Message}");
                    continue;
                }


            }
            neighboursList.Clear();
            localSessions.Clear();
        }

        public static void StopDiscovery()
        {
            shutdown = true;
            if(initComplete)
            {
                nbUdpClient.Close();
            }
        }

        public static void AddSession(int SessionId, int ConnectionId, int LocalPort)
        {
            lock (_sLock)
            {
                localSessions.Add(new GameConnSession()
                {
                    sessionId = SessionId,
                    connectionId = ConnectionId,
                    localport = LocalPort
                });
            }
        }
        public static void AddSession(GameConnSession session)
        {
            bool exists = false;
            lock (_sLock)
            {
                foreach(var existing_session in localSessions)
                {
                    if(existing_session.EqualsConSess(session))
                    {
                        //Session exists, update port if changed.
                        existing_session.localport = session.localport;
                        exists = true;
                        break;
                    }
                }
                if(!exists)
                {
                    localSessions.Add(session);
                }
            }
            SendAdvert();
        }


        public static List<GameConnSession> GetSessions()
        {
            List<GameConnSession> tmpList;
            lock (_sLock)
            {
                tmpList = localSessions.ToList();
            }
            return tmpList;
        }

        public static List<LocalNeighbours> GetList()
        {
            List<LocalNeighbours> tmpList;
            lock(_nLock)
            {
                tmpList = neighboursList.ToList();
            }
            return tmpList;
        }

        static void SendAdvertLoop()
        {
            while (UdpRelay.run_relay)
            {
                SendAdvert();
                Thread.Sleep(1000);
            }
        }
        public static void SendAdvert()
        {
            string message = $"{localStartPort.ToString()},{localStartPort.ToString()}";
            foreach (var local_session in localSessions.ToList())
            {
                message += $",{local_session.sessionId}:{local_session.connectionId}:{local_session.localport}";
            }
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            lock (_sendLock)
            {
                foreach(var bip in GetLocalBroadcastAddresses())
                {
                    nbUdpClient.Send(messageBytes, messageBytes.Length, new IPEndPoint(bip, peerDetectionPort));
                }
                nbUdpClient.Send(messageBytes, messageBytes.Length, new IPEndPoint(IPAddress.Broadcast, peerDetectionPort));
            }
        }

        static List<IPAddress> GetLocalIPAddresses()
        {
            List<IPAddress> localips = new List<IPAddress>();
            try
            {
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface networkInterface in networkInterfaces)
                {
                    if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                        (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                         networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                        networkInterface.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                        foreach (UnicastIPAddressInformation ipAddressInfo in ipProperties.UnicastAddresses)
                        {
                            if (ipAddressInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                localips.Add(ipAddressInfo.Address);
                            }
                        }
                    }
                }
                return localips;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error getting local IP addresses: " + ex.Message);
            }
            return null;
        }

        static List<IPAddress> GetLocalBroadcastAddresses()
        {
            var broadcasts = new List<IPAddress>();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Skip unusable interfaces
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                var ipProps = ni.GetIPProperties();

                foreach (var ua in ipProps.UnicastAddresses)
                {
                    if (ua.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (ua.IPv4Mask == null)
                        continue;

                    var ipBytes = ua.Address.GetAddressBytes();
                    var maskBytes = ua.IPv4Mask.GetAddressBytes();

                    var broadcastBytes = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        broadcastBytes[i] = (byte)(ipBytes[i] | (~maskBytes[i]));
                    }

                    var broadcast = new IPAddress(broadcastBytes);

                    // Avoid duplicates (can happen with multi-IP NICs)
                    if (!broadcasts.Contains(broadcast))
                    {
                        broadcasts.Add(broadcast);
                    }
                }
            }

            return broadcasts;
        }

    }
}
