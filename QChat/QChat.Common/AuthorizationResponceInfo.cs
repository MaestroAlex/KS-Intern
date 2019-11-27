using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct AuthorizationResponceInfo
    {
        public bool Success;

        public static int ByteLength = sizeof(bool);
        
        public void AsBytes(byte[] buff, int offset)
        {
            Array.Copy(BitConverter.GetBytes(Success), 0, buff, offset, sizeof(bool));
        }
        public byte[] AsBytes() => BitConverter.GetBytes(Success);

        public static AuthorizationResponceInfo FromBytes(byte[] buff, int offset) => new AuthorizationResponceInfo
        {
            Success = BitConverter.ToBoolean(buff, offset)
        };

        public static AuthorizationResponceInfo FromConnection<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            if (connection.Read(bytes, 0, ByteLength) <= 0) throw new Exception();
            return FromBytes(bytes, 0);
        }
        public static async Task<AuthorizationResponceInfo> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            if (await connection.ReadAsync(bytes, 0, ByteLength) <= 0) throw new Exception();
            return FromBytes(bytes, 0);
        }
    }
}
