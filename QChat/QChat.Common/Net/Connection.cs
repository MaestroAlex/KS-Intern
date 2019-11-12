using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using QChat.Common;

namespace QChat.Common.Net
{
    public class Connection : IDisposable, IStream
    {
        private TcpClient _tcpClient;
        private readonly NetworkStream _stream;
        private ConnectionManager _manager;

        private bool _disposed = false;

        public ulong Id { get; private set; }

        public Connection(TcpClient tcpClient, ConnectionManager manager)
        {
            _tcpClient = tcpClient;
            _manager = manager;

            _stream = _tcpClient.GetStream();
        }     
        
        public int Read(byte[] buffer, int offset, int length)
        {
            return _stream.Read(buffer, offset, length);
        }
        public async Task<int> ReadAsync(byte[] buffer, int offset, int length)
        {
            return await _stream.ReadAsync(buffer, offset, length);
        }

        public void Write(byte[] buffer, int offset, int length)
        {
            _stream.Write(buffer, offset, length);
        }
        public async Task WriteAsync(byte[] buffer, int offset, int length)
        {
            await _stream.WriteAsync(buffer, offset, length);
        }
        
        public void Dispose()
        {
            if (_disposed) return;

            _tcpClient.Dispose();
            _disposed = true;
        }

        ~Connection()
        {
            Dispose();
        }
    }   
}
