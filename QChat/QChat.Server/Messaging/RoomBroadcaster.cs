using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common;

namespace QChat.Server.Messaging
{
    class RoomBroadcaster
    {
        public void BroadcastToRoom(Message message, Room room)
        {
            var sender = Messenger.DefineSender(message);

            foreach (var user in room.Members)
            {
                foreach (var session in user.Sessions)
                {
                    var connection = session.Connection;

                    Messenger.SendMessageHeader(message.Header, connection);
                    sender.SendContent(connection, message.Content);
                }
            }
        }
        public async Task BroadcastToRoomAsync(Message message, Room room)
        {
            var sender = Messenger.DefineSender(message);

            foreach (var user in room.Members)
            {
                foreach (var session in user.Sessions)
                {
                    var connection = session.Connection;

                    await Messenger.SendMessageHeaderAsync(message.Header, connection);
                    await sender.SendContentAsync(connection, message.Content);
                }
            }
        }
    }
}
