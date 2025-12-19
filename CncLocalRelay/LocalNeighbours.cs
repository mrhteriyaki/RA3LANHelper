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

namespace CncLocalRelay
{
    public class LocalNeighbours
    {
        public IPAddress Address;
        public int StartPort;

        static List<LocalNeighbours> neighboursList = new List<LocalNeighbours>();
        private static readonly object _nLock = new();

        static int localStartPort;
        static UdpClient nbUdpClient = new UdpClient(48632);

        public static void RunDiscovery(int startPort)
        {
            localStartPort = startPort;

            neighboursList.Clear();

            Thread runSend = new Thread(() => SendAdvert());
            runSend.Name = "Neighbours SendAdvert Thread";
            runSend.Start();

            // Allow reuse if multiple apps / restarts
            nbUdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            nbUdpClient.EnableBroadcast = true;

            while (UdpRelay.run_relay)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = nbUdpClient.Receive(ref remoteEP); // BLOCKS until a packet arrives

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
                                if(nb.StartPort != startPortRemote)
                                {
                                    nb.StartPort = startPortRemote; //Port has changed on remote peer.
                                    Trace.WriteLine($"Remote start port has changed on remote peer: {nb.Address}:{nb.StartPort}");
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

        static void SendAdvert()
        {
            while (UdpRelay.run_relay)
            {
                string message = $"{localStartPort.ToString()},{localStartPort.ToString()}";
                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                nbUdpClient.Send(messageBytes, messageBytes.Length, new IPEndPoint(IPAddress.Broadcast, 48632));

                Thread.Sleep(5000);
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



    }
}
