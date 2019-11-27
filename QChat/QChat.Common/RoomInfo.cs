using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RoomInfo
    {
        public int Id;

        public static readonly int ByteLength = sizeof(ulong);


        public byte[] AsBytes()
        {
            return BitConverter.GetBytes(Id);
        }
        public void AsBytes(byte[] buffer, int offset)
        {
            Array.Copy(BitConverter.GetBytes(Id), 0, buffer, offset, sizeof(int));
        }

        public static RoomInfo FromBytes(byte[] buffer, int offset)
        {
            return new RoomInfo { Id = BitConverter.ToInt32(buffer, offset) };
        }
        
        public static RoomInfo FromConnection<T>(T connection) where T : IConnectionStream
        {
            var buffer = new byte[ByteLength];
            if (connection.Read(buffer, 0, ByteLength) <= 0) throw new Exception();
            return FromBytes(buffer, 0);
        }
        public static async Task<RoomInfo> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var buffer = new byte[ByteLength];
            if (await connection.ReadAsync(buffer, 0, ByteLength) <= 0) throw new Exception();
            return FromBytes(buffer, 0);
        }        
    }
}
