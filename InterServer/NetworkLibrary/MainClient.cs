using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLibrary
{
    public class MainClient : NetworkCore
    {
        private TcpClient _tcpClient;

        public delegate void MessageReceived(string message);
        public event MessageReceived MessageEvent;

        public MainClient()
        {
            _tcpClient = new TcpClient();
        }

        public async Task<bool> ConnectToServer(string ip, ushort port)
        {
            bool res = false;
            try
            {
                await _tcpClient.ConnectAsync(ip, port);
                res = true;
                ListenToServer(_tcpClient);
            }
            catch (Exception e)
            {
            }
            return res;
        }

        public async Task<bool> WriteToServer(string message)
        {
            bool res = false;

            try
            {
                byte[] buffer = new byte[message.Length + 4];

                var stream = _tcpClient.GetStream();
                var byteLength = BitConverter.GetBytes(message.Length);

                buffer = byteLength.Concat(Encoding.ASCII.GetBytes(message)).ToArray();

                await stream.WriteAsync(buffer, 0, buffer.Length);

                res = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return res;
        }

        private async void ListenToServer(TcpClient cl)
        {
            while (cl.Connected)
            {
                byte[] buffer = new byte[0];
                try
                {
                    Array.Resize(ref buffer, 4);
                    var dataStream = cl.GetStream();
                    await dataStream.ReadAsync(buffer, 0, 4);
                    var messageLength = BitConverter.ToInt32(buffer, 0);
                    if (messageLength <= 0)
                        return;
                    Array.Resize(ref buffer, messageLength);
                    await dataStream.ReadAsync(buffer, 0, messageLength);
                    var result = Encoding.ASCII.GetString(buffer);

                    MessageEvent?.Invoke(result);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            cl.Close();
        }
    }
}
