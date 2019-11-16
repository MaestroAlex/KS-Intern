using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
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

        public ulong Id { get; private set; }
        public Connection Connection { get; private set; }
        public bool Active { get => _continue; }

        public Session(Connection connection, IManagerProvider managerProvider)
        {
            Id = connection.Id;
            Connection = connection;

            _messenger = managerProvider.Get<MessagingManager>();
            _roomManager = managerProvider.Get<RoomManager>();
        }

        public void Start()
        {
            _continue = true;


            Task.Run(Serve);
        }

        private void Serve()
        {
            try
            {
                while (_continue)
                {
                    var request = RequestHeader.FromConnection(Connection);

                    switch (request.Intention)
                    {
                        case RequestIntention.Messaging:
                            _messenger.HandleMessage(Connection);
                            break;
                        case RequestIntention.Rooming:
                            _roomManager.HandleRooming(Connection);
                            break;
                    }
                }
            }
            catch
            {

            }
        }
    }
}
