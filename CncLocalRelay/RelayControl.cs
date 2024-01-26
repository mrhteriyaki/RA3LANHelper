using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CncLocalRelay
{
    public class RelayControl
    {
        //Use lookup for server.cnc-online.net to resolve IP destination for natneg.server.cnc-online.net.
        //Potential issue can occur if public ip of server.cnc-online.net does not match natneg.server.cnc-online.net in future.
        
        static readonly string server = "server.cnc-online.net";
        static UdpRelay UDPR;

        public static async Task RunRelay(int StartPort, bool UPNP)
        {

            string serverip = string.Empty;
            foreach (IPAddress ip in Dns.GetHostAddresses(server))
            {
                serverip = ip.ToString();
                break;
            }
            if (String.IsNullOrEmpty(serverip))
            {
                Console.WriteLine("Could not get IP for " + server);
                return;
            }

            Console.WriteLine("Starting relay - natneg server ip: " + serverip);

            //await UPNPControl.OpenPortAsync(56000);

            UDPR = new UdpRelay(serverip, 27901, StartPort, UPNP);
            UDPR.RunRelay();
        }

        public static void StopRelay()
        {
            UDPR.StopRelay();
        }

    }
}
