using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RoomConnectionInfo
    {
        public RoomInfo RoomInfo;

        public static int ByteLength = RoomInfo.ByteLength;

        public void AsBytes(byte[] buffer, int offset)
        {
            RoomInfo.AsBytes(buffer, offset);
        }
        public byte[] AsBytes()
        {
            var bytes = new byte[ByteLength];
            AsBytes(bytes, 0);
            return bytes;
        }

        public static RoomConnectionInfo FromBytes(byte[] buffer, int offset) => new RoomConnectionInfo
        {
            RoomInfo = RoomInfo.FromBytes(buffer, offset)
        };

        public static RoomConnectionInfo FromConnection<T>(T connection) where T : IConnectionStream
        {
            var buffer = new byte[ByteLength];
            if (connection.Read(buffer, 0, ByteLength) <= 0) throw new Exception();
            return FromBytes(buffer, 0);
        }
        public static async Task<RoomConnectionInfo> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var buffer = new byte[ByteLength];
            if (await connection.ReadAsync(buffer, 0, ByteLength) <= 0) throw new Exception();
            return FromBytes(buffer, 0);
        }
    }
}
