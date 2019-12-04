using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct AuthorizationResponce
    {
        public AuthorizationResult Result;

        public static readonly int ByteLength = sizeof(bool);
        
        public void AsBytes(byte[] buff, int offset)
        {
            buff[offset] = (byte)Result;
        }
        public byte[] AsBytes() => new byte[]
        {
            (byte)Result
        };

        public static AuthorizationResponce FromBytes(byte[] buff, int offset) => new AuthorizationResponce
        {
            Result = (AuthorizationResult)buff[offset]
        };

        public static AuthorizationResponce FromConnection<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];

            connection.ReadAll(bytes, 0, ByteLength);

            return FromBytes(bytes, 0);
        }
        public static async Task<AuthorizationResponce> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];

            await connection.ReadAsync(bytes, 0, ByteLength);

            return FromBytes(bytes, 0);
        }
    }

    public enum AuthorizationResult : byte
    {
        Success,
        UserNotFound,
        IncorrectPassword,
        Fail
    }
}
