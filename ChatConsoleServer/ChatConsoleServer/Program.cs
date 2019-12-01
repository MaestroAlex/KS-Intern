using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using ChatHandler;

namespace ChatConsoleServer
{
    class Program
    {
        private static int port = 904;
        private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static Hashtable clientsList = new Hashtable();

        static void Main(string[] args)
        {
            #region DLL
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(ipPoint);
            serverSocket.Listen(10);

            Console.WriteLine("Chat Server Started ....");

            while (true)
            {
                var listener = serverSocket.Accept();
                byte[] buffer = new byte[256];
                string userName = null;

                listener.Receive(buffer);

                userName = Encoding.Unicode.GetString(buffer);
                userName = userName.Substring(0, userName.LastIndexOf("$"));

                HandleClinet client = new HandleClinet(listener, userName);
            }
            #endregion
        }

        #region DLL

        #endregion

    }


    #region DLL
    public class HandleClinet
    {
        private readonly Socket _Listener;
        private readonly string _ClientName;
        private static Hashtable _ClientsList = new Hashtable();
        private Hashtable _PersonalChats = new Hashtable();

        public HandleClinet(Socket listener, string clientName)
        {
            _Listener = listener;
            _ClientName = clientName;
            _ClientsList.Add(clientName, listener);
            Broadcast("Joined the room");
            Console.WriteLine($"{clientName} Joined the room.");
            Task.Factory.StartNew(RunChat);
        }

        public void Broadcast(string message, Socket receiver = null)
        {
            if (receiver == null)
            {
                foreach (DictionaryEntry client in _ClientsList)
                {
                    if (client.Value != _Listener)
                    {
                        SendMessage(message, (Socket)client.Value);
                    }
                }
            }
            else
            {
                SendMessage(message, receiver);
            }
        }

        private void SendMessage(string message, Socket receiver)
        {
            byte[] buffer = null;

            message = _ClientName + " : " + message + "$";

            buffer = Encoding.Unicode.GetBytes(message);

            receiver.Send(buffer);
        }

        private void ParseMessage(ref string message)
        {
            message = message.Substring(0, message.LastIndexOf("$"));

            if (message.StartsWith(MsgKeys.NewChat))
            {
                var receiverName = message.Substring(MsgKeys.NewChat.Length);
                receiverName = receiverName.Replace(" ", "");
                if (IsClientExists(receiverName))
                {
                    if (IsChatExists(receiverName))
                    {
                        Broadcast($"Personal chat with {receiverName} already exists.", _Listener);
                    }
                    else
                    {
                        _PersonalChats.Add(receiverName, _ClientsList[receiverName]);

                        Broadcast(MsgKeys.NewChat + _ClientName, (Socket)_PersonalChats[receiverName]);
                        Broadcast(MsgKeys.NewChat + receiverName, _Listener);
                    }
                }
                else
                {
                    Broadcast($"Client {receiverName} doesn't exist", _Listener);
                }
                return;
            }

            if(message.StartsWith(MsgKeys.ToUser))
            {
                var msg = message;
                msg = msg.Substring(MsgKeys.ToUser.Length);
                var receiverName = msg.Substring(0, msg.IndexOf("$"));
                if(IsChatExists(receiverName))
                {
                    Broadcast(msg, (Socket)_PersonalChats[receiverName]);
                }
                else
                {
                    Broadcast($"Chat with {receiverName} is not available",_Listener);
                }
                return;
            }

            Broadcast(message);
        }

        private bool IsClientExists(string clientName)
        {
            bool result = false;
            foreach (DictionaryEntry client in _ClientsList)
            {
                if (client.Key.Equals(clientName))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        private bool IsChatExists(string receiverName)
        {
            bool result = false;
            foreach (DictionaryEntry chat in _PersonalChats)
            {
                if (chat.Key.ToString() == receiverName)
                {
                    result = true;
                }
            }
            return result;
        }

        private void RunChat()
        {
            while (_Listener.Connected)
            {
                try
                {
                    var bytes = _Listener.Available;
                    byte[] buffer = new byte[256];
                    string data = null;
                    _Listener.Receive(buffer);

                    data = Encoding.Unicode.GetString(buffer);

                    ParseMessage(ref data);

                    var message = DateTime.Now.ToString() + " | " + _ClientName + " : " + data;
                    Console.WriteLine(message);
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.ToString());
                    Console.WriteLine(_ClientName + " disconnected");
                    _ClientsList.Remove(_ClientName);
                    _Listener.Close();
                    break;
                }
            }
        }
    }
    #endregion


}
