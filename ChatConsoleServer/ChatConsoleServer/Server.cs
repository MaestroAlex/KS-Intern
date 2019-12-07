using ChatHandler;
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
        private int _Port;//904
        private IPEndPoint _IpPoint;//127.0.0.1
        private TcpListener _ServerSocket;
        private List<HandleClient> _Clients = new List<HandleClient>();
        DBManager _DBManager = DBManager.GetInstance();

        public Server(string IP, int port)
        {
            this._Port = port;
            _IpPoint = new IPEndPoint(IPAddress.Parse(IP), port);
            _ServerSocket = new TcpListener(_IpPoint);
            HandleClient.FillChatsAsync().Wait();

        }

        public async Task StartServer()
        {
            _ServerSocket.Start();

            Console.WriteLine($"Port : {_Port}");
            Console.WriteLine($"IP address : {_IpPoint.Address.ToString()}");
            Console.WriteLine("Chat Server Started ....");

            await StartAcceptingUser();
        }

        private async Task StartAcceptingUser()
        {
            while (true)
            {
                if (_ServerSocket.Pending())
                {
                    try
                    {
                        var listener = _ServerSocket.AcceptTcpClient();

                        
                        var clientStream = listener.GetStream();

                        var size = listener.ReceiveBufferSize;
                        byte[] buffer = new byte[size];
                        string data = null;

                        clientStream.Read(buffer, 0, (int)size);
                        data = Encoding.Unicode.GetString(buffer);
                        data = data.Substring(0, data.LastIndexOf(MsgKeys.End));

                        var parsedData = HandleClient.ParseLogin(data);
                        var name = parsedData[1];
                        var password = parsedData[2];
                        var userAcceptLogin = await _DBManager.AcceptUserLogin(name, password);

                        HandleClient client = new HandleClient(listener, name, password, !userAcceptLogin);

                    }
                    catch (Exception e)
                    {

                    }
                }
            }

        }
    }
}
