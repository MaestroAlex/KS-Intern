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
            if (Connection == null ? false : Connection.Connected) return Connection;

            var tcpClient = new TcpClient();

            try
            {
                tcpClient.Connect(_ipAdress, _port);
            }
            catch
            {
                return null;
            }

            Connection = new Connection(tcpClient, 0);
            Connection.ConnectionClosed += _connectionClosedEventHandler;

            return Connection;
        }
        public async Task<Connection> ConnectAsync()
        {
            if (Connection == null ? false : Connection.Connected) return Connection;

            var tcpClient = new TcpClient();

            try
            {
                await tcpClient.ConnectAsync(_ipAdress, _port);
            }
            catch
            {
                return null;
            }

            Connection = new Connection(tcpClient, 0);
            Connection.ConnectionClosed += _connectionClosedEventHandler;

            return Connection;
        }        

        private void HandleClosedConnection(Connection connection, EventArgs eventArgs)
        {
            Connection = null;
        }
    }
}
