using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RoomInvitationInfo
    {
        public RoomInfo RoomInfo;
        public UserInfo UserInfo;

        public static int ByteLength = RoomInfo.ByteLength + UserInfo.ByteLength;

        public void AsBytes(byte[] buff, int offset)
        {
            RoomInfo.AsBytes(buff, offset);
            offset += RoomInfo.ByteLength;
            UserInfo.AsBytes(buff, offset);
        }
        public byte[] AsBytes()
        {
            var bytes = new byte[ByteLength];
            AsBytes(bytes, 0);
            return bytes;
        }

        public static RoomInvitationInfo FromBytes(byte[] buff, int offset) => new RoomInvitationInfo
        {
            RoomInfo = RoomInfo.FromBytes(buff, offset),
            UserInfo = UserInfo.FromBytes(buff, offset + RoomInfo.ByteLength)
        };

        public static RoomInvitationInfo FromConnection<T>(T connection) where T :  IConnectionStream
        {
            var buff = new byte[ByteLength];

            connection.ReadAll(buff, 0, ByteLength);

            return FromBytes(buff, 0);
        }
        public static async Task<RoomInvitationInfo> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var buff = new byte[ByteLength];

            await connection.ReadAllAsync(buff, 0, ByteLength);

            return FromBytes(buff, 0);
        }
    }
}
