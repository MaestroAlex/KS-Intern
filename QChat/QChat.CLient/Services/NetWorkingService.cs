using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        public IConnection Connection { get; private set; }

        public NetworkingService(IPAddress iPAddress, int port)
        {
            _ipAdress = iPAddress;
            _port = port;
        }


        public IConnection Connect()
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
            Connection.ConnectionClosed += HandleClosedConnection;

            return Connection;
        }
        public async Task<IConnection> ConnectAsync()
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
            Connection.ConnectionClosed += HandleClosedConnection;

            return Connection;
        }        

        private void HandleClosedConnection(IConnection connection, EventArgs eventArgs)
        {
            Connection = null;
            MessageBox.Show("Connection was closed");
        }
    }
}
