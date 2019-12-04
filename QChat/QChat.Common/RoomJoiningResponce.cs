using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RoomJoiningResponce
    {
        public RoomJoiningResult Result;

        public static readonly int ByteLength = sizeof(RoomJoiningResult);


        public RoomJoiningResponce(RoomJoiningResult result)
        {
            Result = result;
        }


        public void AsBytes(byte[] buff, int offset)
        {
            buff[offset] = (byte)Result;
        }
        public byte[] AsBytes() => new byte[]
        {
            (byte)Result
        };

        public static RoomJoiningResponce FromBytes(byte[] buff, int offset) => new RoomJoiningResponce
        {
            Result = (RoomJoiningResult)buff[offset]
        };

        public static RoomJoiningResponce FromConnection<T>(T connection) where T : IConnection
        {
            var bytes = new byte[ByteLength];
            connection.ReadAll(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
        public static async Task<RoomJoiningResponce> FromConnectionAsync<T>(T connection) where T : IConnection
        {
            var bytes = new byte[ByteLength];
            await connection.ReadAllAsync(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
    }

    public enum RoomJoiningResult : byte
    {
        Success,
        RoomIsNotFound,
        RoomIsNotPublic,
        AlreadyMember,
        Fail
    }
}
