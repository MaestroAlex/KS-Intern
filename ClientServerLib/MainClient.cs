using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace ClientServerLib
{
    public class MainClient
    {
        private TcpClient client;
        public delegate void MessageHandler(string message);
        public event MessageHandler onMessageReceived;

        public MainClient()
        {
            client = new TcpClient();
        }

        public async Task<bool> ConnectToServer(string ip, ushort port)
        {
            try
            {
                await client.ConnectAsync(ip, port);
                StartListeningServer();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
            return false;
        }

        public async Task SendToServer(string message)
        {
            try
            {
                byte[] buffer = new byte[message.Length + 4];
                NetworkStream dataStream = client.GetStream();
                byte[] messageLength = BitConverter.GetBytes(message.Length);
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                buffer = messageLength.Concat(messageBytes).ToArray();
                dataStream.Write(buffer, 0, message.Length + 4);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }

        private async Task StartListeningServer()
        {
            while (client.Connected)
            {
                byte[] buffer = new byte[4];
                NetworkStream dataStream = client.GetStream();
                await dataStream.ReadAsync(buffer, 0, 4);
                int length = BitConverter.ToInt32(buffer, 0);
                buffer = new byte[length];
                await dataStream.ReadAsync(buffer, 0, length);
                string message = Encoding.UTF8.GetString(buffer);
                onMessageReceived?.Invoke(message);
            }
            client.Close();
        }
    }
}
