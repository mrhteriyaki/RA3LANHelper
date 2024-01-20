using System;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using Open.Nat;

namespace CncLocalRelay
{
    class UPNPControl
    {
        public static async Task OpenPortAsync(int PortNumber)
        {
            var discoverer = new NatDiscoverer();
            var device = await discoverer.DiscoverDeviceAsync();
            await device.CreatePortMapAsync(new Mapping(Protocol.Udp, PortNumber, PortNumber, "CNCOnline"));
        }

        static IPAddress GetLocalIPAddresses()
        {
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
                                Console.WriteLine($"Interface: {networkInterface.Description}");
                                Console.WriteLine($"IP Address: {ipAddressInfo.Address}");
                                Console.WriteLine();
                                return ipAddressInfo.Address;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting local IP addresses: " + ex.Message);
            }
            return null;
        }
    }


}