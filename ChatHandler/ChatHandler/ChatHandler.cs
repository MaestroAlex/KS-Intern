using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatHandler
{
    public class ClientChatHandler
    {
        private static ClientChatHandler _Instance;
        private static User _User = new User();
        private const int _Port = 904;
        private const string _IpAddress = "192.168.1.108"; //"127.0.0.1";

        public string Username { get => _User.Name; set => _User.Name = value; }
        public string Password { get => _User.Password; set => _User.Password = value; }

        public TcpClient client { get => _User.tcpCLient; }

        private ClientChatHandler() { }

        public static ClientChatHandler Instance()
        {
            if (_Instance == null)
            {
                _Instance = new ClientChatHandler();
            }
            return _Instance;
        }

        public async Task<bool> ConnectUser()
        {
            bool result = false;

            try
            {
                _User.Connect(new TcpClient(_IpAddress, _Port), Username, Password);

                if (_User.tcpCLient.Connected)
                {
                    SendMessage($"{MsgKeys.LogIn}|{Username}|{Password}");

                    var data = ReceiveMessage();
                    if (data.StartsWith(MsgKeys.LogIn) && data.Contains(Username))
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            return result;

        }

        public async void SendMessage(string message, string key = "")
        {
            message = await Encrypt.EncodeMessage(message);
            message += MsgKeys.End;

            var buffer = Encoding.Unicode.GetBytes(message);

            await _User.tcpCLient.GetStream().WriteAsync(buffer, 0, buffer.Length);
        }

        public string ReceiveMessage()
        {
            string result = null;
            try
            {
                if (_User.tcpCLient.Connected)
                {
                    var stream = client.GetStream();
                    var size = client.ReceiveBufferSize;
                    byte[] buffer = new byte[size];
                    int bufferCount = stream.Read(buffer, 0, size);
                    string data = null;
                    data = Encoding.Unicode.GetString(buffer);
                    data = data.Substring(0, data.LastIndexOf(MsgKeys.End));

                    var a = data.Split(MsgKeys.End.ToCharArray());
                    for (int i = 0; i < a.Length; i++)
                    {
                        result += Encrypt.DecodeMessage(a[i]);
                    }

                    //result = await Encrypt.DecodeMessage(data);
                    // data = data.Substring(0, data.LastIndexOf(MsgKeys.End));
                    //result = data;
                }
                else
                {
                    _User.tcpCLient.Close();
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
