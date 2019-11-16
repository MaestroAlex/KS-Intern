using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common;
using QChat.Common.Net;

namespace QChat.Server.Messaging
{
    class RoomManager
    {
        private Dictionary<ulong, Room> _rooms;


        public RoomManager()
        {
            _rooms = new Dictionary<ulong, Room>();
        }

        public RoomingResult HandleRooming(Connection connection)
        {
            throw new NotImplementedException();
        }

        public Room GetRoom(ulong id)
        {
            if (!_rooms.TryGetValue(id, out Room room))
            {
                //TODO: Geting room from DB and adding to cache
            }

            return room;
        }

        public RoomInfo CreateRoom()
        {          
            _rooms.Add(1, new Room(1));
            return new RoomInfo { Id = 1 };
        }
    }
}
