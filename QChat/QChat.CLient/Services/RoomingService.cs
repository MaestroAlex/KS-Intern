using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using QChat.CLient.Rooming;
using QChat.Common.Net;
using QChat.Common;
using System.Windows.Threading;
using QChat.CLient.ViewModels;

namespace QChat.CLient.Services
{
    class RoomingService : IHandler
    {
        private Dictionary<int, Room> _rooms;

        private Dispatcher _dispathcer;

        public event RoomInvitationRecievedEventHandler RoomInvitationRecieved;
        

        public RoomingService()
        {
            _dispathcer = Dispatcher.CurrentDispatcher;
        }

        public void GetRoomsFromServer()
        {
            var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connect();

            Task.WaitAll(
                connection.LockReadAsync(),
                connection.LockWriteAsync()
                );

            _rooms = new Dictionary<int, Room>();

            var requestHeaderBytes = new RequestHeader(RequestIntention.Rooming).AsBytes();
            var roomingHeaderBytes = new RoomingHeader(RoomingIntention.Synchronization).AsBytes();

            try
            {
                connection.Write(requestHeaderBytes, 0, RequestHeader.ByteLength);
                connection.Write(roomingHeaderBytes, 0, RoomingHeader.ByteLength);

                var continueBytes = new byte[sizeof(bool)];
                connection.ReadAll(continueBytes, 0, sizeof(bool));
                var toContinue = BitConverter.ToBoolean(continueBytes, 0);

                while (toContinue)
                {
                    var info = RoomSynchronizationInfo.FromConnection(connection);
                    _rooms.Add(info.Id, new Room(new RoomInfo { Id = info.Id, IsPublic = info.IsPublic }, info.Name.Value));
                    connection.ReadAll(continueBytes, 0, sizeof(bool));
                    toContinue = BitConverter.ToBoolean(continueBytes, 0);
                }
            }
            finally
            {
                connection.ReleaseRead();
                connection.ReleaseWrite();
            }
        }

        public RoomCreationResult CreateRoom(string name, bool isPublic = true)
        {
            var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connect();
            var requestHeaderBytes = new RequestHeader(RequestIntention.Rooming).AsBytes();
            var roomingHeaderBytes = new RoomingHeader(RoomingIntention.Create).AsBytes();
            var roomCreationInfoBytes = new RoomCreationInfo()
            {
                IsPublic = isPublic,
                Name = name
            }.AsBytes();

            Task.WaitAll(
                connection.LockReadAsync(),
                connection.LockWriteAsync()
                );           

            try
            {
                connection.Write(requestHeaderBytes, 0, RequestHeader.ByteLength);
                connection.Write(roomingHeaderBytes, 0, RoomingHeader.ByteLength);
                connection.Write(roomCreationInfoBytes, 0, roomCreationInfoBytes.Length);

                var responce = RoomCreationResponce.FromConnection(connection);

                return responce.Result;
            }
            finally
            {
                connection.ReleaseWrite();
                connection.ReleaseRead();
            }
        }

        public async Task<RoomConnectionResult> ConnectToRoomAsync(int id)
        {
            var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connect();
            var requestHeaderBytes = new RequestHeader(RequestIntention.Rooming).AsBytes();
            var roomingHeaderBytes = new RoomingHeader(RoomingIntention.Create).AsBytes();
            var roomConnectionInfoBytes = new RoomConnectionInfo() { RoomInfo = { Id = id } }.AsBytes();

            Task.WaitAll(
                connection.LockWriteAsync(),
                connection.LockReadAsync()
                );

            try
            {
                connection.Write(requestHeaderBytes, 0, RequestHeader.ByteLength);
                connection.Write(roomingHeaderBytes, 0, RoomingHeader.ByteLength);
                connection.Write(roomConnectionInfoBytes, 0, roomConnectionInfoBytes.Length);

                var responce = RoomConnectionResponce.FromConnection(connection);

                return responce.Result;
            }
            finally
            {
                connection.ReleaseWrite();
                connection.ReleaseRead();
            }
        }
        public RoomConnectionResult ConnectToRoom(int id)
        {
            var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connect();
            var requestHeaderBytes = new RequestHeader(RequestIntention.Rooming).AsBytes();
            var roomingHeaderBytes = new RoomingHeader(RoomingIntention.Connect).AsBytes();
            var roomConnectionInfoBytes = new RoomConnectionInfo() { RoomInfo = { Id = id } }.AsBytes();

            Task.WaitAll(
                connection.LockWriteAsync(),
                connection.LockReadAsync()
                );

            try
            {
                connection.Write(requestHeaderBytes, 0, RequestHeader.ByteLength);
                connection.Write(roomingHeaderBytes, 0, RoomingHeader.ByteLength);
                connection.Write(roomConnectionInfoBytes, 0, roomConnectionInfoBytes.Length);

                var responce = RoomConnectionResponce.FromConnection(connection);


                return responce.Result;
            }
            finally
            {
                connection.ReleaseWrite();
                connection.ReleaseRead();
            }
        }

        public RoomJoiningResult JoinRoom(int id)
        {
            var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connect();
            var requestHeaderBytes = new RequestHeader(RequestIntention.Rooming).AsBytes();
            var roomingHeaderBytes = new RoomingHeader(RoomingIntention.Join).AsBytes();
            var roomInfoBytes = new RoomInfo { Id = id }.AsBytes();

            Task.WaitAll(
                connection.LockWriteAsync(),
                connection.LockReadAsync()
            );

            try
            {
                connection.Write(requestHeaderBytes, 0, RequestHeader.ByteLength);
                connection.Write(roomingHeaderBytes, 0, RoomingHeader.ByteLength);
                connection.Write(roomInfoBytes, 0, RoomInfo.ByteLength);

                var responce = RoomJoiningResponce.FromConnection(connection);
                return responce.Result;                
            }
            finally
            {
                connection.ReleaseWrite();
                connection.ReleaseRead();
            }
        }
        public async Task<RoomJoiningResult> JoinRoomAsync(int id)
        {
            var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connect();
            var requestHeaderBytes = new RequestHeader(RequestIntention.Rooming).AsBytes();
            var roomingHeaderBytes = new RoomingHeader(RoomingIntention.Create).AsBytes();
            var roomInfoBytes = new RoomInfo { Id = id }.AsBytes();

            Task.WaitAll(
                connection.LockWriteAsync(),
                connection.LockReadAsync()
            );

            try
            {
                await connection.WriteAsync(requestHeaderBytes, 0, RequestHeader.ByteLength);
                await connection.WriteAsync(roomingHeaderBytes, 0, RoomingHeader.ByteLength);
                await connection.WriteAsync(roomInfoBytes, 0, RoomInfo.ByteLength);

                var responce = RoomJoiningResponce.FromConnection(connection);
                return responce.Result;
            }
            finally
            {
                connection.ReleaseWrite();
                connection.ReleaseRead();
            }
        }

        public async Task<RoomInvitationResult> InviteToRoomAsync(int userId, int roomId)
        {
            var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connect();
            var requestHeaderBytes = new RequestHeader(RequestIntention.Rooming).AsBytes();
            var roomingHeaderBytes = new RoomingHeader(RoomingIntention.Create).AsBytes();
            var roomInvitationInfoBytes = new RoomInvitationInfo
            {
                RoomInfo = { Id = roomId },
                UserInfo = { Id = userId }
            }.AsBytes();

            Task.WaitAll(
                connection.LockWriteAsync(),
                connection.LockReadAsync()
            );

            try
            {
                await connection.WriteAsync(requestHeaderBytes, 0, RequestHeader.ByteLength);
                await connection.WriteAsync(roomingHeaderBytes, 0, RoomingHeader.ByteLength);
                await connection.WriteAsync(roomInvitationInfoBytes, 0, RoomInvitationInfo.ByteLength);

                var responce = RoomInvitationResponce.FromConnection(connection);

                return responce.Result;
            }
            finally            
            {
                connection.ReleaseWrite();
                connection.ReleaseRead();
            }
        }
        public RoomInvitationResult InviteToRoom(int userId, int roomId)
        {
            var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connect();
            var requestHeaderBytes = new RequestHeader(RequestIntention.Rooming).AsBytes();
            var roomingHeaderBytes = new RoomingHeader(RoomingIntention.Create).AsBytes();
            var roomInvitationInfoBytes = new RoomInvitationInfo
            {
                RoomInfo = { Id = roomId },
                UserInfo = { Id = userId }
            }.AsBytes();

            Task.WaitAll(
                connection.LockWriteAsync(),
                connection.LockReadAsync()
            );

            try
            {
                connection.Write(requestHeaderBytes, 0, RequestHeader.ByteLength);
                connection.Write(roomingHeaderBytes, 0, RoomingHeader.ByteLength);
                connection.Write(roomInvitationInfoBytes, 0, RoomInvitationInfo.ByteLength);

                var responce = RoomInvitationResponce.FromConnection(connection);

                return responce.Result;
            }
            finally
            {
                connection.ReleaseWrite();
                connection.ReleaseRead();
            }
        }

        public void Handle(IConnection connection)
        {
            RoomingResponceHeader roomingResponceHeader;

            try
            {
                roomingResponceHeader = RoomingResponceHeader.FromConnection(connection);
            }
            catch
            {
                return;
            }

            switch (roomingResponceHeader.Intention)
            {
                case RoomingResponceIntention.InvitationNotification:
                    HandleRoomInvitation(connection);
                    break;
            }
        }
        public async Task HandleAsync(IConnection connection)
        {
            RoomingResponceHeader roomingResponceHeader;

            try
            {
                roomingResponceHeader = await RoomingResponceHeader.FromConnectionAsync(connection);
            }
            catch
            {
                return;
            }

            switch (roomingResponceHeader.Intention)
            {
                case RoomingResponceIntention.InvitationNotification:
                    await HandleRoomInvitationAsync(connection);
                    break;
            }
        }

        private void HandleRoomInvitation(IConnection connection)
        {
            var notification = RoomInvitationNotification.FromConnection(connection);

            _rooms.Add(notification.RoomSynchronizationInfo.Id, new Room
                (new RoomInfo
                {
                    Id = notification.RoomSynchronizationInfo.Id,
                    IsPublic = notification.RoomSynchronizationInfo.IsPublic
                },
                notification.RoomSynchronizationInfo.Name.Value));

            _dispathcer.Invoke(
                () => RoomInvitationRecieved?.Invoke(this, new RoomInvitationRecievedEventArgs(notification))
                );
        }
        private async Task HandleRoomInvitationAsync(IConnection connection)
        {
            var roomInvitationInfo = await RoomInvitationInfo.FromConnectionAsync(connection);
        }

        public void UpdateRoomCollection(ObservableCollection<ChatListItemVM> collection)
        {            
            collection.Clear();

            foreach(var room in _rooms.Values)
            {
                collection.Add(new ChatListItemVM(room.Name, room.Id));
            }
        }

        public void Disconnect(int roomId)
        {
            var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connect();

            var requestBytes = new RequestHeader(RequestIntention.Rooming).AsBytes();
            var roomingBytes = new RoomingHeader(RoomingIntention.Disconnect).AsBytes();
            var disconnectionBytes = new RoomDisconnectionInfo(new RoomInfo { Id = roomId }).AsBytes();

            connection.LockWriteAsync().Wait();

            try
            {
                connection.Write(requestBytes, 0, RequestHeader.ByteLength);
                connection.Write(roomingBytes, 0, RoomingHeader.ByteLength);
                connection.Write(disconnectionBytes, 0, RoomDisconnectionInfo.ByteLength);
            }
            finally
            {
                connection.ReleaseWrite();
            }
        }
    }
}
