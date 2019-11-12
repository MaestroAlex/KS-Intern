using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace QChat.Common.Net
{
    public class ConnectionManager
    {
        private TcpListener _tcpListner;
        private int _port;
        private IPAddress _ipAddress;

        private bool _continue = false;

        private Dictionary<ulong, Connection> _connections;

        public ListnerState State { get; private set; }

        public ConnectionManager(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;

            _tcpListner = new TcpListener(ipAddress, port);
        }

        public void Start(int maxConnections)
        {
            _tcpListner.Start();
        }     
        
        public Connection GetConnection()
        {
            var connection =  new Connection(_tcpListner.AcceptTcpClient(), this);
            _connections.Add(connection.Id, connection);
            return connection;
        }
        public async Task<Connection> GetConnectionAsync()
        {
            var connection = new Connection(await _tcpListner.AcceptTcpClientAsync(), this);
            _connections.Add(connection.Id, connection);
            return connection;
        }
        
        public void Stop()
        {
            _tcpListner.Stop();
        }
    }

    public enum ListnerState
    {
        Inactive = 0,
        Active
    }
}
