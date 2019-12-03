using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatConsoleServer
{
    class Server
    {
        private int port;//904
        private IPEndPoint ipPoint;//127.0.0.1
        private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private List<HandleClient> _Clients = new List<HandleClient>();
        

        public Server(string IP, int port)
        {
            this.port = port;
            ipPoint = new IPEndPoint(IPAddress.Parse(IP), port);
            serverSocket.Bind(ipPoint);
        }

        public void StartServer()
        {
            serverSocket.Listen(10);

            Console.WriteLine($"Port : {port}");
            Console.WriteLine($"IP address : {ipPoint.Address.ToString()}");
            Console.WriteLine("Chat Server Started ....");

            while (true)
            {
                var listener = serverSocket.Accept();
                byte[] buffer = new byte[256];
                string userName = null;

                listener.Receive(buffer);

                userName = Encoding.Unicode.GetString(buffer);
                userName = userName.Substring(0, userName.LastIndexOf("$"));

                _Clients.Add(new HandleClient(listener, userName));
            }
        }
    }
}
