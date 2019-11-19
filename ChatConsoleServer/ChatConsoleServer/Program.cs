using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace ChatConsoleServer
{
    class Program
    {
        //private static Socket clientSocket;
        private static int port = 904;
        private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static Hashtable clientsList = new Hashtable();

        static void Main(string[] args)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(ipPoint);
            serverSocket.Listen(10);

            Console.WriteLine("Chat Server Started ....");

            while (true)
            {
                var listener = serverSocket.Accept();
                byte[] buffer = new byte[256];
                string dataFromClient = null;

                listener.Receive(buffer);

                dataFromClient = Encoding.Unicode.GetString(buffer);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.LastIndexOf("$"));

                clientsList.Add(dataFromClient, listener);

                Console.WriteLine(dataFromClient + " Joined chat room ");
                HandleClinet client = new HandleClinet(listener, dataFromClient, clientsList);
            }

        }

        public static void Broadcast(string message, string clientName, bool flag, Socket sender)
        {
            foreach (DictionaryEntry Item in clientsList)
            {
                if (Item.Value != sender)
                {
                    var broadcastSocket = (Socket)Item.Value;
                    
                    byte[] buffer = null;

                    buffer = Encoding.Unicode.GetBytes(clientName + " : " + message);

                    broadcastSocket.Send(buffer);
                }
            }
        }
    }



    public class HandleClinet
    {
        private Socket listener;
        private readonly string clientName;
        private readonly Hashtable clientsList;

        public HandleClinet(Socket listener, string clientName, Hashtable clientsList)
        {
            this.listener = listener;
            this.clientName = clientName;
            this.clientsList = clientsList;
            Task.Factory.StartNew(DoChat);
        }

        private void DoChat()
        {
            while (true)
            {
                try
                {
                    if (listener.Connected)
                    {
                        var bytes = listener.Available;
                        byte[] buffer = new byte[256];
                        string data = null;
                        listener.Receive(buffer);

                        data = Encoding.Unicode.GetString(buffer);
                        Program.Broadcast(data, clientName, false, listener);

                        data = data.Substring(0, data.LastIndexOf("$"));
                        var message = DateTime.Now.ToString() + " | " + clientName + " : " + data;
                        Console.WriteLine(DateTime.Now.ToString() + " | " + clientName + " : " + data);
                       
                        
                    }
                    else
                    {
                        listener.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
        }
    }

}
