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
            

            
        }

        public MessagingResult HandleMessage(IConnection connection)
        {
            MessageHeader header;

            try
            {
                header = MessageHeader.FromConnection(connection);
            }
            catch
            {
                return new MessagingResult { Success = false };
            }

            switch (header.RecieverInfo.Type)
            {
                case RecieverType.Room:
                    return _roomMessenger.Handle(connection, header);
                default:
                    throw new NotImplementedException();
            }
        }
        public async Task<MessagingResult> HandleMessageAsync(IConnection connection)
        {
            MessageHeader header;

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
