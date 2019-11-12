using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct AuthorizationInfo
    {
        public UserInfo UserInfo;

        public static readonly int ByteLength = UserInfo.ByteLength;

        public static AuthorizationInfo FromBytes(byte[] buffer, int offset)
        {
            return new AuthorizationInfo() { UserInfo = UserInfo.FromBytes(buffer, offset) };
        }

        public static AuthorizationInfo FromConnection<T>(T connection) where T : Connection
        {
            var buffer = new byte[ByteLength];
            connection.Read(buffer, 0, ByteLength);
            return FromBytes(buffer, 0);
        }
        public static async Task<AuthorizationInfo> FromConnectionAsync<T>(T connection) where T : Connection
        {
            var buffer = new byte[ByteLength];
            await connection.ReadAsync(buffer, 0, ByteLength);
            return FromBytes(buffer, 0);
        }

        public byte[] AsBytes()
        {
            return UserInfo.AsBytes();
        }
        public void AsBytes(byte[] buffer, int offset)
        {
            UserInfo.AsBytes(buffer, offset);
        }
    }
}
