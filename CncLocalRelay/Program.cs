using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace CncLocalRelay
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //Check for firewall allow.

            //Prompt for port offset.

            //Add UPNP Port forwards without crash.

            //Add static hostname record 127.0.0.1 natneg.server.cnc-online.net

            //Need to cleanup exceptions on receive.
            //Cleanup session Lists - close removal.

    


            //Investigate why LAN-discovery fails, but works on BFME2?

            //Relay TCP Ports 80, 6667, 18840, 28910, 29900
            //80 = Http Nginx server.

            //18840 = Encrypted SSL 3.0 (TLS Cert issued 2023-05-22 to 2033-05-17) - Possible EOL issue.

            //DNS: master.server.cnc-online.net -> Linked to UDP 27900, TCP 28910.

            //DNS: natneg.server.cnc-online.net


            //Relay Port UDP 27900


            //Relay and filter port 27910 to replace WAN IPs.

            string targetServer = "185.17.144.132";

            //TCPRelayServer relay1 = new TCPRelayServer(targetServer, 28910);
            //relay1.RunRelay();


            //Relay UDP Traffic to master.server.cnc-online.net

            //Client uses dynamic port + 6613 observed to send traffic, may need to create multiple relays per client source port.

            //UdpRelay UDPR = new UdpRelay(targetServer, 27900);
            //UDPR.RunRelay();

            //await UPNPControl.OpenPortAsync(56000);

            int PortOffset = int.Parse(args[0]);
            UdpRelay UDPR = new UdpRelay(targetServer, 27901, PortOffset);
            UDPR.RunRelay();
        }


    }
}
