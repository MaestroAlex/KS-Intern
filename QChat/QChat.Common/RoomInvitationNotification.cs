using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RoomInvitationNotification
    {
        public UserInfo UserInfo;
        public RoomSynchronizationInfo RoomSynchronizationInfo;

        public int ByteLength => UserInfo.ByteLength + RoomSynchronizationInfo.ByteLength;
        

        public RoomInvitationNotification(UserInfo userInfo, RoomSynchronizationInfo synchronizationInfo)
        {
            UserInfo = userInfo;
            RoomSynchronizationInfo = synchronizationInfo;
        }
        public RoomInvitationNotification(int userId, RoomSynchronizationInfo synchronizationInfo)
        {
            UserInfo = new UserInfo { Id = userId };
            RoomSynchronizationInfo = synchronizationInfo;
        }
        public RoomInvitationNotification(UserInfo userInfo, int roomId, string roomName, bool isPublic)
        {
            UserInfo = userInfo;
            RoomSynchronizationInfo = new RoomSynchronizationInfo(isPublic, roomId, roomName);
        }
        public RoomInvitationNotification(int userId, int roomId, string roomName, bool isPublic)
        {
            UserInfo = new UserInfo { Id = userId };
            RoomSynchronizationInfo = new RoomSynchronizationInfo(isPublic, roomId, roomName);
        }


        public void AsBytes(byte[] buff, int offset)
        {
            UserInfo.AsBytes(buff, offset);
            offset += UserInfo.ByteLength;
            RoomSynchronizationInfo.AsBytes(buff, offset);
        }
        public byte[] AsBytes()
        {
            var bytes = new byte[ByteLength];
            AsBytes(bytes, 0);
            return bytes;
        }

        public static RoomInvitationNotification FromBytes(byte[] bytes, int offset) => new RoomInvitationNotification
        {
            UserInfo = UserInfo.FromBytes(bytes, offset),
            RoomSynchronizationInfo = RoomSynchronizationInfo.FromBytes(bytes, offset + UserInfo.ByteLength)
        };

        public static RoomInvitationNotification FromConnection<T>(T connection) where T : IConnectionStream => new RoomInvitationNotification
        {
            UserInfo = UserInfo.FromConnection(connection),
            RoomSynchronizationInfo = RoomSynchronizationInfo.FromConnection(connection)
        };
        public static async Task<RoomInvitationNotification> FromConnectionAsync<T>(T connection) where T : IConnectionStream => new RoomInvitationNotification
        {
            UserInfo = await UserInfo.FromConnectionAsync(connection),
            RoomSynchronizationInfo = await RoomSynchronizationInfo.FromConnectionAsync(connection)
        };
    }
}
