using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Net.Sockets;

namespace ClientServerLib.ClientAndServer
{
    public class ClientServerBase
    {
        protected async Task<byte[]> GetMessageBytes(string message)
        {
            byte[] buffer = new byte[message.Length + 4];
            byte[] messageLength = BitConverter.GetBytes(message.Length);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            buffer = messageLength.Concat(messageBytes).ToArray();
            return buffer;
        }

        protected async Task SendMessageToClient(TcpClient socket, byte[] buffer)
        {
            NetworkStream dataStream = socket.GetStream();
            await dataStream.WriteAsync(buffer, 0, buffer.Length);
        }

        protected async Task<string> WaitMessage(TcpClient socket)
        {
            byte[] buffer = new byte[4];
            NetworkStream dataStream = socket.GetStream();
            await dataStream.ReadAsync(buffer, 0, 4);
            int length = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[length];
            await dataStream.ReadAsync(buffer, 0, length);
            return Encoding.UTF8.GetString(buffer);
        }
    }
}
