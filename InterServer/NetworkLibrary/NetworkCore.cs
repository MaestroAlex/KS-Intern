using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLibrary
{
    public class NetworkCore
    {
        public delegate void MessageReceived(string message, object sender);
        protected event MessageReceived MessageEvent;

        public NetworkCore()
        {
            
        }

        protected async Task ListenToClient(ClientEssence client)
        {
            var cl = client.Socket;
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
                    Debug.WriteLine("Received from client: " + result);
                    MessageEvent?.Invoke(result, client);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            cl.Close();
        }

        public async Task<bool> WriteToClient(ClientEssence client, string message)
        {
            bool res = false;
            var cl = client.Socket;
            try
            {
                byte[] buffer = new byte[message.Length + 4];

                var stream = cl.GetStream();
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
    }
}
