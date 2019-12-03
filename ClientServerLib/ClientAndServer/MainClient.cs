using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using ClientServerLib.Additional;

namespace ClientServerLib.ClientAndServer
{
    public class MainClient : ClientServerBase
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
                string ecnryptedMessage = await Crypto.Encrypt(message);
                await base.SendMessageToClient(client, await base.GetMessageBytes(ecnryptedMessage));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }

        private async Task StartListeningServer()
        {
            try
            {
                while (client.Connected)
                {
                    string message = await base.WaitMessage(client);
                    string decryptedMessage = await Crypto.Decrypt(message);
                    onMessageReceived?.Invoke(decryptedMessage);
                }
                client.Close();
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
