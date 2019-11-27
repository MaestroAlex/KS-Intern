using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct UserInfo
    {
        public int Id;

        public static readonly int ByteLength = sizeof(int);       

        public static UserInfo Null { get => new UserInfo() { Id = 0 }; }

        public static UserInfo FromBytes(byte[] buffer, int offset) => new UserInfo
        {
            Id = BitConverter.ToInt32(buffer, offset)
        };
        

        public byte[] AsBytes() => BitConverter.GetBytes(Id);
        public void AsBytes(byte[] buffer, int offset)
        {
            var bytes = BitConverter.GetBytes(Id);
            Array.Copy(bytes , 0, buffer, offset, sizeof(int));
        }

        public static UserInfo FromConnection<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            if (connection.Read(bytes, 0, ByteLength) <= 0) throw new Exception();
            return new UserInfo
            {
                Id = BitConverter.ToInt32(bytes, 0)
            };
        }
        public static async Task<UserInfo> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            if (await connection.ReadAsync(bytes, 0, ByteLength) <= 0) throw new Exception();
            return new UserInfo
            {
                Id = BitConverter.ToInt32(bytes, 0)
            };
        }
    }
}
