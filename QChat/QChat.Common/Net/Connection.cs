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
        public bool Connected => _tcpClient.Connected;
        public int DataAvailable => _tcpClient.Available;

        public event ConnectionClosedEventHandler ConnectionClosed;

        public Connection(TcpClient tcpClient, int id)
        {
            _tcpClient = tcpClient;
            Id = id;
            _tcpClient.ReceiveTimeout = 20000;
            _tcpClient.SendTimeout = 20000;

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
            catch(IOException e)
            {
                Close();
                throw e;
            }
        }
        public async Task<int> ReadAsync(byte[] buffer, int offset, int length)
        {
            try
            {
                //.NET 4.7.2 NetworkStream's implementation of ReadAsync is not overriden and base "Stream" implementation doesn't work properly,
                //so transition from APM to TAP pattern is used
                return await Task<int>.Factory.FromAsync(_stream.BeginRead,
                    _stream.EndRead, buffer,
                    offset, length, null);
            }
            catch (Exception e)
            {
                if (e.InnerException is IOException)
                {
                    Close();
                }
                throw e.InnerException;
            }
        }

        public void Write(byte[] buffer, int offset, int length)
        {
            try
            {
                _stream.Write(buffer, offset, length);
            }
            catch (IOException e)
            {
                Close();
                throw e;
            }
        }
        public async Task WriteAsync(byte[] buffer, int offset, int length)
        {
            try
            {
                await Task.Factory.FromAsync(_stream.BeginWrite,
                    _stream.EndWrite, buffer, offset, length,
                    null);
            }             
            catch (Exception e)
            {
                if (e.InnerException is IOException) Close();
                throw e.InnerException;
            }
}

        public void ReadAll(byte[] buffer, int offset, int length)
        {
            while (length > 0)
            {
                var byteCount = Read(buffer, offset, length);
                offset += byteCount;
                length -= byteCount;
            }            
        }
        public async Task ReadAllAsync(byte[] buffer, int offset, int length)
        {
            while (length > 0)
            {
                var byteCount = await ReadAsync(buffer, offset, length);
                offset += byteCount;
                length -= byteCount;
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

        public bool WaitForData(int miliseconds)
        {
            if (DataAvailable > 0) return true;

            Thread.Sleep(miliseconds);

            return DataAvailable > 0;
        }

        public void Close()
        {
            _tcpClient.Close();
            Dispose(false);
        }

        public void Dispose()
        {
            Close();
        }
        
        public void Dispose(bool disposing)
        {
            if (_disposed) return;
            
            _tcpClient.Dispose();
            if (disposing)
            {
                _readLock.Dispose();
                _writeLock.Dispose();
            }
            ConnectionClosed?.Invoke(this, null);
            _disposed = true;
        }

        ~Connection()
        {
            Dispose(true);
        }
    }

    public delegate void ConnectionClosedEventHandler(IConnection sender, EventArgs eventArgs);
}
