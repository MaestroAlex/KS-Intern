using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using QChat.Common;

namespace QChat.Common.Net
{
    public class Connection : IConnection
    {
        private TcpClient _tcpClient;
        private readonly NetworkStream _stream;

        private SemaphoreSlim _readLock;
        private SemaphoreSlim _writeLock;

        private bool _disposed = false;

        public int Id { get; private set; }
        public bool Connected { get; private set; }

        public event ConnectionClosedEventHandler ConnectionClosed;

        public Connection(TcpClient tcpClient, int id)
        {
            _tcpClient = tcpClient;
            Id = id;

            _readLock = new SemaphoreSlim(1, 1);
            _writeLock = new SemaphoreSlim(1, 1);

            _stream = _tcpClient.GetStream();
        }

        public int Read(byte[] buffer, int offset, int length)
        {
            try
            {
                return _stream.Read(buffer, offset, length);
            }
            catch
            {
                _tcpClient.Close();
                Dispose();
                return -1;
            }
        }
        public async Task<int> ReadAsync(byte[] buffer, int offset, int length)
        {
            try
            {
                return await _stream.ReadAsync(buffer, offset, length);
            }
            catch
            {

                _tcpClient.Close();
                Dispose();
                return -1;
            }
        }

        public bool Write(byte[] buffer, int offset, int length)
        {
            try
            {
                _stream.Write(buffer, offset, length);
                return true;
            }
            catch
            {
                _tcpClient.Close();
                Dispose();
                return false;
            }
        }
        public async Task<bool> WriteAsync(byte[] buffer, int offset, int length)
        {
            try
            {
                await _stream.WriteAsync(buffer, offset, length);
                return true;
            }
            catch
            {
                _tcpClient.Close();
                Dispose();
                return false;
            }
        }

        public void LockRead()
        {
            _readLock.Wait();
        }
        public Task LockReadAsync()
        {
            return _readLock.WaitAsync();
        }
        public void ReleaseRead()
        {
            _readLock.Release();
        }

        public void LockWrite()
        {
            _writeLock.Wait();
        }
        public Task LockWriteAsync()
        {
            return _writeLock.WaitAsync();
        }
        public void ReleaseWrite()
        {
            _writeLock.Release();
        }

        public void Dispose()
        {
            if (_disposed) return;

            Connected = false;

            _tcpClient.Dispose();
            _readLock.Dispose();
            _writeLock.Dispose();
            ConnectionClosed?.Invoke(this, null);
            _disposed = true;
        }

        ~Connection()
        {
            Dispose();
        }
    }

    public delegate void ConnectionClosedEventHandler(Connection sender, EventArgs eventArgs);
}
