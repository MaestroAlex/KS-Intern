using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using QChat.Common.Net;
using QChat.Common;
using QChat.Server.Sessioning;

namespace QChat.Server.Messaging
{
    class MessagingManager
    {
        private IMessenger<Room> _roomMessenger;
        private IMessenger<Group> _groupMessenger;
        private IMessenger<User> _userMessenger;

        public MessagingManager(IManagerProvider managerProvider, ContentRecieverTable recievers, ContentSenderTable senders)
        {
            _roomMessenger = new RoomMessenger(managerProvider, new ContentSenderTable(), new ContentRecieverTable());

            //TODO: Other chat messengers
        }

        public MessagingResult HandleMessage(Session session)
        {
            MessageHeader header;
            var connection = session.Connection;                
                
            header = MessageHeader.FromConnection(connection);

            switch (header.RecieverInfo.Type)
            {
                case RecieverType.Room:
                    return _roomMessenger.Handle(connection, header);
                default:
                    return new MessagingResult();
            }
                
        }
        public async Task<MessagingResult> HandleMessageAsync(Session session)
        {
            MessageHeader header;
            var connection = session.Connection;

            try
            {
                header = await MessageHeader.FromConnectionAsync(connection);
            }
            catch
            {
                return new MessagingResult { Success = false };
            }

            switch (header.RecieverInfo.Type)
            {
                case RecieverType.Room:
                    return await _roomMessenger.HandleAsync(connection, header);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
