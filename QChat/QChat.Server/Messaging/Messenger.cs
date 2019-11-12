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
    class Messenger
    {
        private GroupManager _groupManager;
        private UserManager _userManager;
        private RoomManager _roomManager;

        private static RecieverTable _recievers;
        private static SenderTable _senders;

        public Messenger(GroupManager groupManager, UserManager userManager, RoomManager roomManager, RecieverTable recievers, SenderTable senders)
        {
            _groupManager = groupManager;
            _userManager = userManager;
            _roomManager = roomManager;
            _recievers = recievers;
            _senders = senders;
        }

        public MessagingResult HandleMessage(Connection connection)
        {
            var result = new MessagingResult();
            var header = MessageHeader.FromConnection(connection);
            Content content;

            try
            {
                switch (header.ContentType)
                {
                    case ContentType.Text: content = _recievers.Get<TextReciever>().GetContent(header, connection);                
                        break;
                }
            }
            catch
            {
                result.Success = false;
                return result;
            }

            var message = new Message(header, content);

            switch (header.RecieverInfo.Type)
            {
                case RecieverType.Room:
                    var room = _roomManager.GetRoom(header.RecieverInfo.Id);
                    room?.Broadcast(message);

                    break;
            }

            result.Success = true;
            return result;
        }

        public void SendMessage(Message message, User user)
        {
            var sender = DefineSender(message);

            foreach(var session in user.Sessions)
            {
                sender.SendContent(session.Connection, message.Content);
            }
        }
        public async Task SendMessageAsync(Message message, User user)
        {
            var sender = DefineSender(message);

            foreach (var session in user.Sessions)
            {
                await sender.SendContentAsync(session.Connection, message.Content);
            }
        }

        private void SendMessage(Message message, User user, ISender sender)
        {
            foreach (var session in user.Sessions)
            {
                sender.SendContent(session.Connection, message.Content);
            }
        }

        public static ISender DefineSender(Message message)
        {
            ISender sender;

            switch (message.Header.ContentType)
            {
                case ContentType.Text:
                    sender = _senders.GetSender<TextSender>();
                    break;
                default:
                    sender = _senders.GetSender<TextSender>();
                    break;
            }

            return sender;
        }

        public static void SendMessageHeader(MessageHeader header, Connection connection)
        {
            var bytes = header.AsBytes();
            connection.Write(bytes, 0, MessageHeader.ByteLength);
        }
        public static async Task SendMessageHeaderAsync(MessageHeader header, Connection connection)
        {
            var bytes = header.AsBytes();
            await connection.WriteAsync(bytes, 0, MessageHeader.ByteLength);
        }
    }
}
