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
        public bool IsPublic;

        public static readonly int ByteLength = sizeof(ulong) + sizeof(bool);


        public byte[] AsBytes()
        {
            var bytes = new byte[ByteLength];
            AsBytes(bytes, 0);
            return bytes;
        }
        public void AsBytes(byte[] buffer, int offset)
        {
            Array.Copy(BitConverter.GetBytes(Id), 0, buffer, offset, sizeof(int));
            offset += sizeof(int);
            Array.Copy(BitConverter.GetBytes(IsPublic), 0, buffer, offset, sizeof(bool));
        }

        public static RoomInfo FromBytes(byte[] buffer, int offset)
        {
            var result = new RoomInfo();

            result.Id = BitConverter.ToInt32(buffer, offset);
            offset += sizeof(int);
            result.IsPublic = BitConverter.ToBoolean(buffer, offset);

            return result;
        }
        
        public static RoomInfo FromConnection<T>(T connection) where T : IConnectionStream
        {
            var buffer = new byte[ByteLength];
            connection.ReadAll(buffer, 0, ByteLength);
            return FromBytes(buffer, 0);
        }
        public static async Task<RoomInfo> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var buffer = new byte[ByteLength];
            await connection.ReadAllAsync(buffer, 0, ByteLength);
            return FromBytes(buffer, 0);
        }        
    }
}
