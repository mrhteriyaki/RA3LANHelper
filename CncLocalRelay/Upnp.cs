using System;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using SharpOpenNat;
using System.Diagnostics;


namespace CncLocalRelay
{
    class UPNPControl
    {
        public static async Task OpenPortAsync(int PortNumber)
        {
            var device = await NatDiscoverer.DiscoverDeviceAsync();
            await device.CreatePortMapAsync(new Mapping(Protocol.Udp, PortNumber, PortNumber, "CNCOnline"));
            Trace.WriteLine("Opening port with UPNP: " + PortNumber);
        }
        public static async Task ClosePortAsync(int PortNumber)
        {
            var device = await NatDiscoverer.DiscoverDeviceAsync();
            await device.DeletePortMapAsync(new Mapping(Protocol.Udp, PortNumber, PortNumber, "CNCOnline"));

            Trace.WriteLine("Removing UPNP Port: " + PortNumber);
        }

    }


}