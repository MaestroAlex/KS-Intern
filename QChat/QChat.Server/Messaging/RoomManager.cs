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

        private RoomDataManager _roomDataManager;
        private UserDataManager _userDataManager;

        public RoomManager()
        {
            _roomDataManager = new RoomDataManager();
            _userDataManager = new UserDataManager();

            _rooms = _roomDataManager.GetRooms();
        }

        public void HandleRooming(Session session)
        {
            var connection = session.Connection;
            var header = RoomingHeader.FromConnection(connection);

            switch (header.Intention)
            {
                case RoomingIntention.Connect:
                    ConnectUserToRoom(session);
                    break;
                case RoomingIntention.Disconnect:
                    DisconnectUserFromRoom(session);
                    break;
                case RoomingIntention.Create:
                    CreateRoom(session);
                    break;
                case RoomingIntention.Join:
                    JoinUserToRoom(session);
                    break;
                case RoomingIntention.Remove:
                    RemoveRoom(session);
                    break;
                case RoomingIntention.Invitation:
                    InviteUserToRoom(session);                    
                    break;
                case RoomingIntention.Synchronization:
                    SynchronizeUser(session);
                    break;
                default:
                    break;
            }
        }
        public async Task HandleRoomingAsync(Session session)
        {
            var connection = session.Connection;
            var header = await RoomingHeader.FromConnectionAsync(connection);

            switch (header.Intention)
            {
                case RoomingIntention.Connect:
                    await ConnectUserToRoomAsync(session);
                    break;
                case RoomingIntention.Create:
                    await CreateRoomAsync(session);
                    break;
                case RoomingIntention.Join:
                    await JoinUserToRoomAsync(session);
                    break;
                case RoomingIntention.Remove:
                    await RemoveRoomAsync(session);
                    break;
                case RoomingIntention.Invitation:
                    await InviteUserToRoomAsync(session);
                    break;
                case RoomingIntention.Synchronization:
                    await SynchronizeUserAsync(session);
                    break;
                default:
                    break;
            }
        }

        public void ConnectUserToRoom(Session session)
        {
            var connection = session.Connection;

            try
            {
                var info = RoomConnectionInfo.FromConnection(connection);
                byte[] responceBytes;

                if (!_rooms.ContainsKey(info.RoomInfo.Id))
                {
                    responceBytes = new RoomConnectionResponce(RoomConnectionResult.RoomNotFound).AsBytes();
                    connection.Write(responceBytes, 0, RoomConnectionResponce.ByteLength);
                    return;
                }
                if (!_rooms[info.RoomInfo.Id].AddActiveMember(session))
                {
                    responceBytes = new RoomConnectionResponce(RoomConnectionResult.UserIsNotMemberOfRoom).AsBytes();
                    connection.Write(responceBytes, 0, RoomConnectionResponce.ByteLength);
                    return;
                }

                responceBytes = new RoomConnectionResponce(RoomConnectionResult.UserIsNotMemberOfRoom).AsBytes();
                connection.Write(responceBytes, 0, RoomConnectionResponce.ByteLength);
                return;
            }
            catch(Exception e)
            {
                return;
            }
        }
        public async Task ConnectUserToRoomAsync(Session session)
        {
            var connection = session.Connection;

            try
            {
                var info = await RoomConnectionInfo.FromConnectionAsync(connection);

                if (!_rooms.ContainsKey(info.RoomInfo.Id))
                {
                    var responceBytes = new RoomConnectionResponce(RoomConnectionResult.RoomNotFound).AsBytes();
                    await connection.WriteAsync(responceBytes, 0, RoomConnectionResponce.ByteLength);
                    return;
                }
                if (!_rooms[info.RoomInfo.Id].AddActiveMember(session))
                {
                    var responceBytes = new RoomConnectionResponce(RoomConnectionResult.UserIsNotMemberOfRoom).AsBytes();
                    await connection.WriteAsync(responceBytes, 0, RoomConnectionResponce.ByteLength);
                    return;
                }
            }
            catch
            {
                return;
            }
        }

        public void JoinUserToRoom(Session session)
        {
            var connection = session.Connection;

            try
            {
                var info = RoomInfo.FromConnection(connection);

                if (!_rooms.ContainsKey(info.Id))
                {
                    var responceBytes = new RoomJoiningResponce(RoomJoiningResult.RoomIsNotFound).AsBytes();
                    connection.Write(responceBytes, 0, RoomJoiningResponce.ByteLength);
                    return;
                }

                var room = _rooms[info.Id];

                if (room.HasMember(session.UserId))
                {
                    var responceBytes = new RoomJoiningResponce(RoomJoiningResult.AlreadyMember).AsBytes();
                    connection.Write(responceBytes, 0, RoomJoiningResponce.ByteLength);

                    return;
                }

                if (room.IsPublic)
                {
                    room.AddMember(new UserInfo { Id = session.UserId });
                    _roomDataManager.AddMemberToRoom(session.UserId, room.Id);

                    var responceBytes = new RoomJoiningResponce(RoomJoiningResult.Success).AsBytes();
                    connection.Write(responceBytes, 0, RoomJoiningResponce.ByteLength);

                    Task.Run(
                        () => NotifyUserInvited(
                            new UserInfo { Id = session.Id },
                            room.Id,
                            room.Name,
                            room.IsPublic,
                            connection)
                        );
                    return;
                }
                else
                {
                    var responceBytes = new RoomJoiningResponce(RoomJoiningResult.RoomIsNotPublic).AsBytes();
                    connection.Write(responceBytes, 0, RoomJoiningResponce.ByteLength);
                    return;
                }
            }
            catch
            {
                
            }
        }
        public async Task JoinUserToRoomAsync(Session session)
        {
            var connection = session.Connection;

            try
            {
                var info = await RoomInfo.FromConnectionAsync(connection);

                if (!_rooms.ContainsKey(info.Id))
                {
                    var responceBytes = new RoomJoiningResponce(RoomJoiningResult.RoomIsNotFound).AsBytes();
                    await connection.WriteAsync(responceBytes, 0, RoomJoiningResponce.ByteLength);
                    return;
                }

                var room = _rooms[info.Id];

                if (room.IsPublic)
                {
                    room.AddMember(new UserInfo { Id = session.UserId });
                    var responceBytes = new RoomJoiningResponce(RoomJoiningResult.Success).AsBytes();
                    await connection.WriteAsync(responceBytes, 0, RoomJoiningResponce.ByteLength);
                    return;
                }
                else
                {
                    var responceBytes = new RoomJoiningResponce(RoomJoiningResult.RoomIsNotPublic).AsBytes();
                    await connection.WriteAsync(responceBytes, 0, RoomJoiningResponce.ByteLength);
                    return;
                }
            }
            catch
            {

            }
        }


        public void CreateRoom(Session session)
        {
            var connection = session.Connection;

            try
            {
                var creationInfo = RoomCreationInfo.FromConnection(connection);

                var roomId = _roomDataManager.RegisterRoom(creationInfo.Name, creationInfo.IsPublic);

                if (roomId == -1)
                {
                    var responceBytes = new RoomCreationResponce(RoomCreationResult.Fail).AsBytes();
                    connection.Write(responceBytes, 0, RoomCreationResponce.ByteLength);
                    return;
                }
                else
                {
                    _rooms.Add(roomId, new Room(
                       roomId,
                       creationInfo.Name,
                       creationInfo.IsPublic,
                       new UserInfo[] { new UserInfo { Id = session.UserId } }
                       ));

                    _roomDataManager.AddMemberToRoom(session.UserId, roomId);

                    var responceBytes = new RoomCreationResponce(RoomCreationResult.Success).AsBytes();
                    connection.Write(responceBytes, 0, RoomCreationResponce.ByteLength);

                    Task.Run(() => NotifyUserInvited(new UserInfo
                        {
                            Id = session.UserId
                        },
                        roomId,
                        creationInfo.Name,
                        creationInfo.IsPublic,
                        connection
                        )
                    );
                    return;
                }                
            }
            catch(Exception e) { }            
        }
        public async Task CreateRoomAsync(Session session)
        {
            var connection = session.Connection;

            try
            {
                var creationInfo = await RoomCreationInfo.FromConnectionAsync(connection);

                var roomId = await _roomDataManager.RegisterRoomAsync(creationInfo.Name);

                if (roomId == -1)
                {
                    var responceBytes = new RoomCreationResponce(RoomCreationResult.Fail).AsBytes();
                    await connection.WriteAsync(responceBytes, 0, RoomCreationResponce.ByteLength);
                    return;
                }
                else
                {
                    _rooms.Add(roomId, new Room(
                       roomId,
                       creationInfo.Name,
                       creationInfo.IsPublic,
                       new UserInfo[] { new UserInfo { Id = session.UserId } }
                       ));

                    var responceBytes = new RoomCreationResponce(RoomCreationResult.Success).AsBytes();
                    await connection.WriteAsync(responceBytes, 0, RoomCreationResponce.ByteLength);
                    
                    return;
                }
            }
            catch { }
        }

        public void RemoveRoom(Session session)
        {

        }
        public async Task RemoveRoomAsync(Session session)
        {

        }

        public void InviteUserToRoom(Session session)
        {
            var connection = session.Connection;

            try
            {
                var info = RoomInvitationInfo.FromConnection(connection);

                var responce = new RoomInvitationResponce();

                if (!_rooms.TryGetValue(info.RoomInfo.Id, out var room))
                {
                    responce.Result = RoomInvitationResult.RoomNotFound;
                    connection.Write(responce.AsBytes(), 0, RoomInvitationResponce.ByteLength);
                    return;
                }

                if (!room.IsPublic && !room.HasMember(session.UserId))
                {
                    responce.Result = RoomInvitationResult.NoPermission;
                    connection.Write(responce.AsBytes(), 0, RoomInvitationResponce.ByteLength);
                    return;
                }

                if (!_userDataManager.UserRegistered(info.UserInfo.Id))
                {
                    responce.Result = RoomInvitationResult.UserNotFound;
                    connection.Write(responce.AsBytes(), 0, RoomInvitationResponce.ByteLength);
                    return;
                }

                if (!_roomDataManager.AddMemberToRoom(info.UserInfo.Id, info.RoomInfo.Id))
                {
                    responce.Result = RoomInvitationResult.InnerError;
                    connection.Write(responce.AsBytes(), 0, RoomInvitationResponce.ByteLength);
                    return;
                }

                room.AddMember(info.UserInfo);

                Task.Run(
                    () => NotifyUserInvitedAsync(
                    info.UserInfo,
                    room.Id,
                    room.Name,
                    room.IsPublic,
                    connection)
                );

                responce.Result = RoomInvitationResult.Success;
                connection.Write(responce.AsBytes(), 0, RoomInvitationResponce.ByteLength);
                return;
            }
            catch
            {

            }
        }
        public async Task InviteUserToRoomAsync(Session session)
        {
            var connection = session.Connection;

            try
            {
                var info = await RoomInvitationInfo.FromConnectionAsync(connection);

                var responce = new RoomInvitationResponce();

                if (!_rooms.TryGetValue(info.RoomInfo.Id, out var room))
                {
                    responce.Result = RoomInvitationResult.RoomNotFound;
                    await connection.WriteAsync(responce.AsBytes(), 0, RoomInvitationResponce.ByteLength);
                    return;
                }

                if (!room.IsPublic && !room.HasMember(session.UserId))
                {
                    responce.Result = RoomInvitationResult.NoPermission;
                    await connection.WriteAsync(responce.AsBytes(), 0, RoomInvitationResponce.ByteLength);
                    return;
                }

                if (!_userDataManager.UserRegistered(info.UserInfo.Id))
                {
                    responce.Result = RoomInvitationResult.UserNotFound;
                    await connection.WriteAsync(responce.AsBytes(), 0, RoomInvitationResponce.ByteLength);
                    return;
                }

                if (!_roomDataManager.AddMemberToRoom(info.UserInfo.Id, info.RoomInfo.Id))
                {
                    responce.Result = RoomInvitationResult.InnerError;
                    await connection.WriteAsync(responce.AsBytes(), 0, RoomInvitationResponce.ByteLength);
                    return;
                }

                room.AddMember(info.UserInfo);

                Task.Run(
                    () => NotifyUserInvited(
                    info.UserInfo,
                    room.Id,
                    room.Name,
                    room.IsPublic,
                    connection)
                );

                responce.Result = RoomInvitationResult.Success;
                connection.Write(responce.AsBytes(), 0, RoomInvitationResponce.ByteLength);
                return;
            }
            catch
            {

            }
        }

        public void SynchronizeUser(Session session)
        {
            var connection = session.Connection;

            var continueBytes = BitConverter.GetBytes(true);

            foreach(var info in _roomDataManager.GetUserRooms(session.UserId))
            {
                connection.Write(continueBytes, 0, sizeof(bool));
                var infoBytes = info.AsBytes();
                connection.Write(infoBytes, 0, infoBytes.Length);
            }

            connection.Write(BitConverter.GetBytes(false), 0, sizeof(bool));            
        }
        public async Task SynchronizeUserAsync(Session session)
        {
            var connection = session.Connection;

            var continueBytes = BitConverter.GetBytes(true);

            foreach (var info in await _roomDataManager.GetUserRoomsAsync(session.UserId))
            {
                await connection.WriteAsync(continueBytes, 0, sizeof(bool));
                var infoBytes = info.AsBytes();
                await connection.WriteAsync(infoBytes, 0, infoBytes.Length);
            }

            await connection.WriteAsync(BitConverter.GetBytes(false), 0, sizeof(bool));
        }

        public Room GetRoom(int id)
        {
            _rooms.TryGetValue(id, out var room);
            return room;
        }

        private void NotifyUserInvited(UserInfo userInfo, int roomId, string roomName, bool isPublic, IConnection connection)
        {
            var responceHeaderBytes = new ResponceHeader(ResponceIntention.Rooming).AsBytes();
            var roomingResponceHeaderBytes = new RoomingResponceHeader(RoomingResponceIntention.InvitationNotification).AsBytes();
            var notificationBytes = new RoomInvitationNotification(userInfo, roomId, roomName, isPublic).AsBytes();

            connection.LockWrite();

            connection.Write(responceHeaderBytes, 0, ResponceHeader.ByteLength);
            connection.Write(roomingResponceHeaderBytes, 0, RoomingResponceHeader.ByteLength);
            connection.Write(notificationBytes, 0, notificationBytes.Length);

            connection.ReleaseWrite();
        }
        private async void NotifyUserInvitedAsync(UserInfo userInfo, int roomId, string roomName, bool isPublic, IConnection connection)
        {
            var responceHeaderBytes = new ResponceHeader(ResponceIntention.Rooming).AsBytes();
            var roomingResponceHeaderBytes = new RoomingResponceHeader(RoomingResponceIntention.InvitationNotification).AsBytes();
            var notificationBytes = new RoomInvitationNotification(userInfo, roomId, roomName, isPublic).AsBytes();

            await connection.LockWriteAsync();

            await connection.WriteAsync(responceHeaderBytes, 0, ResponceHeader.ByteLength);
            await connection.WriteAsync(roomingResponceHeaderBytes, 0, RoomingResponceHeader.ByteLength);
            await connection.WriteAsync(notificationBytes, 0, notificationBytes.Length);

            connection.ReleaseWrite();
        }

        private void DisconnectUserFromRoom(Session session)
        {
            var disconnectInfo = RoomDisconnectionInfo.FromConnection(session.Connection);

            if (_rooms.TryGetValue(disconnectInfo.RoomInfo.Id, out var room) 
                &&
                room.MemberConnected(session.UserId, session.Id))
            {
                room.DisconnectMember(session.UserId, session.Id);
            }
        }
    }
}
