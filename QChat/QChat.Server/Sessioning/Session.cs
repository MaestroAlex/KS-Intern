using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using QChat.Common.Net;
using QChat.Server.Messaging;
using QChat.Common;

namespace QChat.Server.Sessioning
{
    class Session
    {
        private bool _continue = false;

        private MessagingManager _messenger;
        private RoomManager _roomManager;

        public int UserId { get; private set; }
        public int Id { get; private set; }
        public IConnection Connection { get; private set; }
        public bool Active { get => _continue; }

        public event SessionClosedEventHandler SessionClosed;

        public Session(IConnection connection, IManagerProvider managerProvider, UserInfo userInfo)
        {
            Id = connection.Id;
            Connection = connection;
            UserId = userInfo.Id;
            connection.ConnectionClosed += HandleClosedConnection;

            _messenger = managerProvider.Get<MessagingManager>();
            _roomManager = managerProvider.Get<RoomManager>();
        }

        public void Start()
        {
            _continue = true;


            Task.Run(Serve);
        }

        private async void Serve()
        {
            try
            {
                var silenceCount = 0;

                while (_continue && Connection.Connected)
                {
                    Task.WaitAll(
                        Connection.LockReadAsync(),
                        Connection.LockWriteAsync()
                        );

                    try
                    {
                        if (Connection.WaitForData(200))
                        {
                            silenceCount = 0;

                            var request = RequestHeader.FromConnection(Connection);

                            switch (request.Intention)
                            {
                                case RequestIntention.Messaging:
                                    _messenger.HandleMessage(this);
                                    break;
                                case RequestIntention.Rooming:
                                    _roomManager.HandleRooming(this);
                                    break;
                            }
                        }
                        else
                        {
                            silenceCount += 200;

                            if (silenceCount >= 1000 * 15)
                            {
                                Connection.Write(
                                 new ResponceHeader(ResponceIntention.ConnectionCheck).AsBytes(),
                                 0,
                                 ResponceHeader.ByteLength);
                                silenceCount = 0;
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        if (e is IOException)
                            throw e;
                    }
                    finally
                    {
                        Connection.ReleaseRead();
                        Connection.ReleaseWrite();
                    }
                }
            }
            catch(Exception e)
            {

            }

            Connection.Close();
        }

        private void HandleClosedConnection(IConnection sender, EventArgs args)
        {
            _continue = false;
            SessionClosed?.Invoke(this, null);
        }
    }

    delegate void SessionClosedEventHandler(Session sender, EventArgs eventArgs);
}
