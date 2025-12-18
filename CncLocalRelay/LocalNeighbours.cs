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
        public static List<LocalNeighbours> neighboursList = new List<LocalNeighbours>();
        public IPAddress Address;
        public int StartPort;

        static int localStartPort;

        static UdpClient client = new UdpClient(48632);

        public static void RunDiscovery(int startPort)
        {
            localStartPort = startPort;

            Thread runSend = new Thread(() => SendAdvert());
            runSend.Start();

            // Allow reuse if multiple apps / restarts
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.EnableBroadcast = true;

            //client.Client.Bind(new IPEndPoint(IPAddress.Any, 0));

            // Blocking receive loop
            while (UdpRelay.run_relay)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref remoteEP); // BLOCKS until a packet arrives

                bool skipLocal = false;
                foreach (var localIP in GetLocalIPAddresses())
                {
                    if (remoteEP.Address.Equals(localIP))
                    {
                        skipLocal = true;
                        break;
                    }
                }
                if(remoteEP.Address.Equals(IPAddress.Loopback))
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
                //ProcessPacket(remoteEP, data);
                if(!dataString.Contains(","))
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
                    foreach (var nb in neighboursList)
                    {
                        if (nb.Address.Equals(remoteEP.Address) && nb.StartPort == startPortRemote)
                        {
                            nExists = true;
                            break;
                        }
                    }
                    if (!nExists)
                    {
                        Trace.WriteLine($"Neighbour Detected: {remoteEP.Address.ToString()}:{startPortRemote}");
                        neighboursList.Add(new LocalNeighbours()
                        {
                            Address = remoteEP.Address,
                            StartPort = startPortRemote
                        });
                    }
                }
                catch
                {
                    continue;
                }

                
            }
        }

        static void SendAdvert()
        {
            while(UdpRelay.run_relay)
            {
                string message = $"{localStartPort.ToString()},{localStartPort.ToString()}";
                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                client.Send(messageBytes, messageBytes.Length, new IPEndPoint(IPAddress.Broadcast, 48632));

                Thread.Sleep(5000);
            }
        }

        static List<IPAddress> GetLocalIPAddresses()
        {
            List<IPAddress> localips = new List<IPAddress>();
            try
            {
                // Get all network interfaces on the machine
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface networkInterface in networkInterfaces)
                {
                    // Consider only operational and IPv4 capable interfaces
                    if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                        (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                         networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                        networkInterface.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        // Get the IP properties of the interface
                        IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

                        // Get the unicast addresses (IP addresses) associated with the interface
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
