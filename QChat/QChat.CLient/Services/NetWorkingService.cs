using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using QChat.Common.Net;
using QChat.Common;

namespace QChat.CLient.Services
{
    class NetworkingService
    {
        private IPAddress _ipAdress;
        private int _port;

        private AuthorizationService _authorizationService;

        public Connection Connection { get; private set; } 

        private ConnectionClosedEventHandler _connectionClosedEventHandler;


        public NetworkingService(IPAddress iPAddress, int port)
        {
            _ipAdress = iPAddress;
            _port = port;

            _connectionClosedEventHandler = HandleClosedConnection;
        }


        public Connection Connect()
        {
            if (Connection != null) return Connection;

            var tcpClient = new TcpClient();

            try
            {
                tcpClient.Connect(_ipAdress, _port);
            }
            catch
            {
                return null;
            }

            Connection = new Connection(tcpClient);
            Connection.ConnectionClosed += _connectionClosedEventHandler;

            if (!_authorizationService.Authorize(Connection)) Connection = null;

            return Connection;
        }
        public async Task<Connection> ConnectAsync()
        {
            if (Connection != null) return Connection;

            var tcpClient = new TcpClient();

            try
            {
                await tcpClient.ConnectAsync(_ipAdress, _port);
            }
            catch
            {
                return null;
            }

            Connection = new Connection(tcpClient);
            Connection.ConnectionClosed += _connectionClosedEventHandler;

            if (!await _authorizationService.AuthorizeAsync(Connection)) Connection = null;

            return Connection;
        }

        private void HandleClosedConnection(Connection connection, EventArgs eventArgs)
        {
            Connection = null;
        }
    }
}
