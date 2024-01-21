using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace CncLocalRelay
{
    public class Program
    {
        
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Acts as a relay to natneg.server.cnc-online.net to allow access for multiple clients on the same LAN.");
                Console.WriteLine("Arguments: Starting port number.");
                return;
            }
            
            int StartPort = int.Parse(args[0]);

            bool upnp = false;
            if(args.Length == 2)
            {
                if (args[1].Equals("upnp"))
                {
                    upnp = true;
                }
            }

            RelayControl.RunRelay(StartPort, upnp);
        }

        

    }
}
