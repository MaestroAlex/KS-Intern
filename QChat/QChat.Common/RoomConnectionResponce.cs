using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RoomConnectionResponce
    {
        public RoomConnectionResult Result;

        public static int ByteLength = sizeof(RoomConnectionResult);

        public RoomConnectionResponce(RoomConnectionResult result)
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

        public static RoomConnectionResponce FromBytes(byte[] bytes, int offset) => new RoomConnectionResponce
        {
            Result = (RoomConnectionResult)bytes[offset]
        };

        public static RoomConnectionResponce FromConnection<T>(T connection) where T : IConnection
        {
            var bytes = new byte[ByteLength];
            connection.ReadAll(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
        public static async Task<RoomConnectionResponce> FromConnectionAsync<T>(T connection) where T : IConnection
        {
            var bytes = new byte[ByteLength];
            await connection.ReadAllAsync(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
    }

    public enum RoomConnectionResult : byte
    {
        Success,
        RoomNotFound,
        UserIsNotMemberOfRoom,
        Fail
    }
}
