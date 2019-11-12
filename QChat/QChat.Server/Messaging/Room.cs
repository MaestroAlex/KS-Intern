using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Server.Sessioning;
using QChat.Common;

namespace QChat.Server.Messaging
{
    class Room
    {
        private RoomBroadcaster _broadcaster;

        public ulong Id { get; private set; }
        public List<User> Members { get; private set; }


        public Room(RoomBroadcaster broadcaster, ulong id)
        {
            _broadcaster = broadcaster;
            Id = id;
            Members = new List<User>();
        }
        public Room(RoomBroadcaster broadcaster, ulong id, IEnumerable<User> members)
        {
            _broadcaster = broadcaster;
            Id = id;
            Members = new List<User>(members);
        }

        public void Broadcast(Message message) => _broadcaster.BroadcastToRoom(message, this);
        public async Task BroadcastAsync(Message message) => await _broadcaster.BroadcastToRoomAsync(message, this);
    }
}
