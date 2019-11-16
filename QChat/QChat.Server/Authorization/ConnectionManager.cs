using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using QChat.Common.Net;

namespace QChat.Server.Authorization
{
    public class ConnectionManager
    {
        private readonly TcpListener _tcpListner;
        private int _maxConnections;
        private ConnectionClosedEventHandler _connectionClosedEventHandler;

        private readonly Dictionary<ulong, IConnection> _connections;


        public ListnerState State { get; private set; }


        public ConnectionManager(IPAddress ipAddress, int port)
        {
            _tcpListner = new TcpListener(ipAddress, port);
            _connections = new Dictionary<ulong, IConnection>();

            _connectionClosedEventHandler = HandleClosedConnection;
        }

        public void Start(int maxConnections)
        {
            _maxConnections = maxConnections;
            _tcpListner.Start();
        }     
        
        public Connection GetConnection()
        {
            var connection =  new Connection(_tcpListner.AcceptTcpClient());
            connection.ConnectionClosed += _connectionClosedEventHandler;
            _connections.Add(connection.Id, connection);
            return connection;
        }
        public async Task<Connection> GetConnectionAsync()
        {
            var connection = new Connection(await _tcpListner.AcceptTcpClientAsync());
            connection.ConnectionClosed += _connectionClosedEventHandler;
            _connections.Add(connection.Id, connection);
            return connection;
        }

        private void HandleClosedConnection(Connection connection, EventArgs eventArgs)
        {
            
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
