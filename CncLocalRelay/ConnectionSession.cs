using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CncLocalRelay
{
    public class ConnectionSession
    {
        public IPEndPoint client;
        public IPEndPoint remoteServer;
        public UdpClient udpClient;
        int _LocalPort;

        public ConnectionSession(IPAddress IP, int Port, int LocalPort)
        {
            _LocalPort = LocalPort;
            client = new IPEndPoint(IP, Port);
            udpClient = new UdpClient(_LocalPort);
            remoteServer = new IPEndPoint(IPAddress.Any, 0);
        }
        public int GetPort()
        {
            return _LocalPort;  
        }

        public bool IsNatNeg()
        {
            if(remoteServer.Address.Equals(UdpRelay.NatNegRealServer))
            {
                return true;
            }
            return false;
        }



    }


}
