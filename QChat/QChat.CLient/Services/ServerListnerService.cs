using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using QChat.Common;
using QChat.Common.Net;
using System.IO;

namespace QChat.CLient.Services
{
    class ServerListnerService
    {
        private NetworkingService _networkingService;
        
        public bool Listning { get; private set; }

        public ServerListnerService()
        {
            _networkingService = StaticProvider.GetInstanceOf<NetworkingService>();
        }

        public async Task ListenToServer()
        {
            var connection = await _networkingService.ConnectAsync();
            connection.ConnectionClosed += HandleClosedConnection;
            Listning = true;

            try
            {
                while (Listning)
                {
                    Task.WaitAll(
                        connection.LockReadAsync(),
                        connection.LockWriteAsync()
                        );

                    try
                    {
                        if (connection.WaitForData(200))
                        {
                            var header = ResponceHeader.FromConnection(connection);

                            var handler = ChooseHandler(header);
                            handler?.Handle(connection);
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is IOException || e is SocketException)
                            throw e;
                    }
                    finally
                    {
                        connection.ReleaseRead();
                        connection.ReleaseWrite();
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private void HandleClosedConnection(IConnection connection, EventArgs args)
        {
            Listning = false;
        }

        private IHandler ChooseHandler(ResponceHeader header)
        {
            switch (header.Intention)
            {
                case ResponceIntention.Messaging:
                    return StaticProvider.GetInstanceOf<MessagingService>();
                case ResponceIntention.Rooming:
                    return StaticProvider.GetInstanceOf<RoomingService>();
                default:
                    return null;
            }
        }
    }
}
