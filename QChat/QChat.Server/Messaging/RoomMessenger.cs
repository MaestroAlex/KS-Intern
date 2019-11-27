using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common;
using QChat.Common.Net;

namespace QChat.Server.Messaging
{
    class RoomMessenger : Messenger<Room>
    {
        private RoomManager _roomManager;


        public RoomMessenger(IManagerProvider managerProvider, ContentSenderTable senders, ContentRecieverTable recievers) : base(senders, recievers)
        {
            _roomManager = managerProvider.Get<RoomManager>();
        }


        public override MessagingResult Handle(IConnection connection, MessageHeader header)
        {
            var message = RecieveMessage(connection, header);
            if (message == null) return new MessagingResult { Success = false };

            var room = _roomManager.GetRoom(header.RecieverInfo.Id);
            SendMessage(room, message);

            return new MessagingResult { Success = true };
        }
        public override async Task<MessagingResult> HandleAsync(IConnection connection, MessageHeader header)
        {
            var message = await RecieveMessageAsync(connection, header);
            if (message == null) return new MessagingResult { Success = false };

            var room = _roomManager.GetRoom(header.RecieverInfo.Id);
            await SendMessageAsync(room, message);

            return new MessagingResult { Success = true };
        }

        protected override void SendMessage(Room room, Message message)
        {
            var sender = DefineSender(message);

            foreach (var userSessions in room.GetMembersSessions())
            {
                foreach (var session in userSessions.Values)
                {
                    var connection = session.Connection;

                    connection.LockWrite();

                    SendMessageResponce(connection);
                    SendMessageHeader(message.Header, connection);
                    sender.SendContent(connection, message.Content);

                    connection.ReleaseWrite();
                }
            }
        }
        protected override async Task SendMessageAsync(Room room, Message message)
        {
            var sender = DefineSender(message);

            foreach (var sessions in room.GetMembersSessions())
            {
                foreach (var session in sessions.Values)
                {
                    var connection = session.Connection;

                    await connection.LockWriteAsync();

                    await SendMessageResponceAsync(connection);
                    await SendMessageHeaderAsync(message.Header, connection);
                    await sender.SendContentAsync(connection, message.Content);

                    connection.ReleaseWrite();
                }
            }
        }

        protected override Message RecieveMessage(IConnection connection, MessageHeader header)
        {
            var reciver = DefineReciever(header);

            try
            {
                return new Message(header, reciver.GetContent(header, connection));
            }
            catch
            {
                return null;
            }
        }
        protected override async Task<Message> RecieveMessageAsync(IConnection connection, MessageHeader header)
        {
            var reciver = DefineReciever(header);

            try
            {
                return new Message(header, await reciver.GetContentAsync(header, connection));
            }
            catch
            {
                return null;
            }
        }

        private void SendMessageResponce(IConnection connection)
        {
            connection.Write(
                new ResponceHeader(ResponceIntention.Messaging).AsBytes(),
                0,
                ResponceHeader.ByteLength);
        }
        private async Task SendMessageResponceAsync(IConnection connection)
        {
            await connection.WriteAsync(
                new ResponceHeader(ResponceIntention.Messaging).AsBytes(),
                0,
                ResponceHeader.ByteLength);
        }
    }
}
