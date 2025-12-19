using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CncLocalRelay
{
    public class RelayControl
    {
        //Use lookup for server.cnc-online.net to resolve IP destination for natneg.server.cnc-online.net.
        //Potential issue can occur if public ip of server.cnc-online.net does not match natneg.server.cnc-online.net in future, it is currently a CNAME record for server.cnc-online.net.
        
        static readonly string server = "server.cnc-online.net";
        static UdpRelay UDPR;

        public static async Task RunRelay(int StartPort, bool UPNP)
        {

            string natneg_server = string.Empty;
            foreach (IPAddress ip in Dns.GetHostAddresses(server))
            {
                natneg_server = ip.ToString();
                break;
            }
            if (String.IsNullOrEmpty(natneg_server))
            {
                Trace.WriteLine("Could not get IP for " + server);
                return;
            }

            try
            {
                PublicIP.Update();
                Trace.WriteLine($"Public IP Detected: {PublicIP.GetString()}");

                Thread NDisThread = new Thread(() => LocalNeighbours.RunDiscovery(StartPort));
                NDisThread.Name = "NDisThread";
                NDisThread.Start();
            }
            catch(Exception ex)
            {
                Trace.WriteLine("Could not get Public IP.");
            }

            Trace.WriteLine("Starting relay - natneg server ip: " + natneg_server);
            UdpRelay.NatNegRealServer = IPAddress.Parse(natneg_server);
            UdpRelay.run_relay = true;
            UDPR = new UdpRelay(StartPort, UPNP);
            UDPR.Relay();

            
        }

        public static void StopRelay()
        {
            if(UDPR != null)
            {
                UDPR.StopRelay();
            }
        }

    }
}
