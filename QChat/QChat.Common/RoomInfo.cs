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
        public ulong Id;


        public static readonly int ByteLength = sizeof(ulong);



        public byte[] AsBytes()
        {
            return BitConverter.GetBytes(Id);
        }
        public void AsBytes(byte[] buffer, int offset)
        {
            Array.Copy(BitConverter.GetBytes(Id), 0, buffer, offset, sizeof(ulong));
        }

        public static RoomInfo FromBytes(byte[] buffer, int offset)
        {
            return new RoomInfo { Id = BitConverter.ToUInt64(buffer, offset) };
        }
        
        public static RoomInfo FromConnection<T>(T connection) where T : Connection
        {
            var buffer = new byte[ByteLength];
            connection.Read(buffer, 0, ByteLength);
            return FromBytes(buffer, 0);
        }
        public static async Task<RoomInfo> FromConnectionAsync<T>(T connection) where T : Connection
        {
            var buffer = new byte[ByteLength];
            await connection.ReadAsync(buffer, 0, ByteLength);
            return FromBytes(buffer, 0);
        }        
    }
}
