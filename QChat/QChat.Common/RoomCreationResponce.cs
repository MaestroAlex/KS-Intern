using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RoomCreationResponce
    {
        public RoomCreationResult Result;

        public static readonly int ByteLength = sizeof(RoomConnectionResult);


        public RoomCreationResponce(RoomCreationResult result)
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

        public static RoomCreationResponce FromBytes(byte[] bytes, int offset) => new RoomCreationResponce
        {
            Result = (RoomCreationResult)bytes[offset]
        };

        public static RoomCreationResponce FromConnection<T>(T connection) where T : IConnection
        {
            var bytes = new byte[ByteLength];
            connection.ReadAll(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
        public static async Task<RoomCreationResponce> FromConnectionAsync<T>(T connection) where T : IConnection
        {
            var bytes = new byte[ByteLength];
            await connection.ReadAllAsync(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
    }

    public enum RoomCreationResult : byte
    {
        Success,
        Fail
    }
}
