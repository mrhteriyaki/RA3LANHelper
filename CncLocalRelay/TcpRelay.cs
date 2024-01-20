using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CncLocalRelay
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class TCPRelayServer
    {
        IPAddress remoteIPAddress;
        int _relayPort;
        TcpListener listener;
        bool run_relay = true;

        public TCPRelayServer(string RelayIP, int RelayPort)
        {
            _relayPort = RelayPort;
            remoteIPAddress = IPAddress.Parse(RelayIP);
        }


        public void RunRelay()
        {
            listener = new TcpListener(IPAddress.Any, _relayPort);
            listener.Start();

            Thread RelayLoopThread = new Thread(RelayLoop);
            RelayLoopThread.Start();
        }

        void RelayLoop()
        {
            try
            {
                while (run_relay)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Thread HandleClientThread = new Thread(HandleClient);
                    HandleClientThread.Start(client);
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        void HandleClient(object clientObj)
        {
            TcpClient client = (TcpClient)clientObj;
            try
            {
                TcpClient remoteServer = new TcpClient(remoteIPAddress.ToString(), _relayPort);

                try
                {
                    NetworkStream clientStream = client.GetStream();
                    NetworkStream remoteStream = remoteServer.GetStream();

                    Thread relayInThread = new Thread(() => RelayIn(clientStream, remoteStream));
                    relayInThread.Start();

                    Thread relayOutThread = new Thread(() => RelayOut(clientStream,remoteStream));
                    relayOutThread.Start();


                    while (client.Connected)
                    {
                        Thread.Sleep(1); //Required to prevent connection close while transfer occuring between relays.
                    }
                   
                }
                finally
                {
                    //client.Close();
                    remoteServer.Close();
                }
            }
            catch
            {
                //TcpClient excepton will occur if unable to connect to remoteIPAddress.
                Console.WriteLine($"Failed to connect to target server {remoteIPAddress}");
            }
        }

        void RelayIn(NetworkStream source, NetworkStream destination)
        { 

            byte[] buffer = new byte[4096];
            int bytesRead;
            try
            {
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    destination.Write(buffer, 0, bytesRead);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Relay in error: {ex.Message}");
            }
        }

        void RelayOut(NetworkStream source, NetworkStream destination)
        {
            byte[] buffer = new byte[4096];
            int bytesRead;
            try
            {
                while ((bytesRead = destination.Read(buffer, 0, buffer.Length)) > 0)
                {
                    source.Write(buffer, 0, bytesRead);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Relay out error: {ex.Message}");
            }
        }

        void StopRelay()
        {
            run_relay = false;
        }
    }

}
