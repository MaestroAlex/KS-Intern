using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatHandler
{
    class User
    {
        string _Name;
        Socket _Socket;
        public User(Socket socket=null, string name = "")
        {
            _Name = name;
            _Socket = socket;
        }

        public string Name { get => _Name; set => _Name = value; }
        public Socket Socket { get => _Socket; private set => _Socket = value; }
    }
    public struct MsgKeys 
    {
        public static readonly string ToEveryone = "$e";
        public static readonly string ToUser = "$u";
        public static readonly string LogIn = "$l";
        public static readonly string NewChat = "$n";
    }

    public class ClientChatHandler
    {
        private static ClientChatHandler _Instance;
        private static User _User = new User();
        private const int _Port = 904;
        private const string _IpAddress = "127.0.0.1";

        public string UserName
        {
            get
            {
                return _User.Name;
            }
            set
            {
                _User.Name = value;
            }
        }

        public Socket socket
        {
            get
            {
                return _User.Socket;
            }
        }

        private ClientChatHandler() { }

        public static ClientChatHandler Instance()
        {
            if (_Instance == null)
            {
                _Instance = new ClientChatHandler();
            }
            return _Instance;
        }

        public async Task ConnectUser(string userName)
        {

            _User = new User(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), userName);
            try
            {
                _User.Socket.Connect(_IpAddress, _Port);
                _User.Socket.Send(Encoding.Unicode.GetBytes(_User.Name + MsgKeys.LogIn));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

        }

        public async Task SendMessage(string message, string key)
        {
            _User.Socket.Send(Encoding.Unicode.GetBytes(key + message + "$"));
        }

        public string ReceiveMessage()
        {
            string result = null;
            try
            {
                if (_User.Socket.Connected)
                {
                    var bytes = _User.Socket.Available;
                    byte[] buffer = new byte[256];
                    string data = null;
                    _User.Socket.Receive(buffer);
                    data = Encoding.Unicode.GetString(buffer);
                    data = data.Substring(0, data.LastIndexOf("$"));
                    result = data;
                }
                else
                {
                    _User.Socket.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result;
        }
    }
}
