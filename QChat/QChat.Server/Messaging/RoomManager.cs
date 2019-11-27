using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common;
using QChat.Common.Net;
using QChat.Server.Sessioning;
using QChat.Server.DataManagment;

namespace QChat.Server.Messaging
{
    class RoomManager
    {
        private Dictionary<int, Room> _rooms;

        private RoomDataManager _dataManager;


        public RoomManager()
        {
            _rooms = new Dictionary<int, Room>();
            _dataManager = new RoomDataManager();
        }

        public RoomingResult HandleRooming(Session session)
        {
            var connection = session.Connection;
            var header = RoomingHeader.FromConnection(connection);

            switch (header.Intention)
            {
                case RoomingIntention.Connect:
                    return ConnectUserToRoom(session, ref header);
                case RoomingIntention.Create:
                    return CreateRoom(session, ref header);
                case RoomingIntention.Join:
                    return JoinUserToRoom(session, ref header);
                case RoomingIntention.Remove:
                    return RemoveRoom(session, ref header);
                default:
                    return new RoomingResult();
            }
        }        

        public RoomingResult ConnectUserToRoom(Session session, ref RoomingHeader header)
        {
            var info = RoomConnectionInfo.FromConnection(session.Connection);

            if (!_rooms.ContainsKey(info.RoomInfo.Id)) return new RoomingResult { Info = info.RoomInfo, Success = false };            
            if (!_rooms[info.RoomInfo.Id].AddActiveMember(session)) return new RoomingResult { Info = info.RoomInfo, Success = false };

            return new RoomingResult { Info = info.RoomInfo, Success = true };
        }

        public RoomingResult JoinUserToRoom(Session session, ref RoomingHeader header)
        {
            var info = RoomInfo.FromConnection(session.Connection);
            
            if (!_rooms.ContainsKey(info.Id)) return new RoomingResult { Info = info, Success = false };

            var room = _rooms[info.Id];

            if (room.IsPublic)
            {
                room.AddMember(new UserInfo { Id = session.UserId });
                return new RoomingResult { Info = info, Success = true };
            }

            return new RoomingResult { Info = info, Success = false };
        }

        public RoomingResult CreateRoom(Session session, ref RoomingHeader header)
        {
            var creationInfo = RoomCreationInfo.FromConnection(session.Connection);

            if (_dataManager.RoomRegistered(creationInfo.Name)) return new RoomingResult
            {
                Success = false
            };

            var roomId = _dataManager.RegisterRoom(creationInfo.Name);

            if (roomId == -1) return new RoomingResult
            {
                Success = false
            };

            _rooms.Add(roomId, new Room(
                roomId, 
                creationInfo.IsPublic, 
                new UserInfo[] { new UserInfo { Id = session.UserId } }
                ));

            return new RoomingResult
            {
                Info = { Id = roomId },
                Success = true
            };
        }

        public RoomingResult RemoveRoom(Session session, ref RoomingHeader header)
        {
            throw new NotImplementedException();
        }

        public Room GetRoom(int id)
        {
            _rooms.TryGetValue(id, out var room);
            return room;
        }
    }
}
