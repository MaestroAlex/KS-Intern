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
        public int PasswordHash;

        public static readonly int ByteLength = UserInfo.ByteLength + sizeof(int);

        public static AuthorizationInfo FromBytes(byte[] buffer, int offset) => new AuthorizationInfo
        {
            UserInfo = UserInfo.FromBytes(buffer, offset),
            PasswordHash = BitConverter.ToInt32(buffer, offset + UserInfo.ByteLength),
        };

        public static AuthorizationInfo FromConnection<T>(T connection) where T : IConnectionStream
        {
            var buffer = new byte[ByteLength];
            connection.ReadAll(buffer, 0, ByteLength);
            return FromBytes(buffer, 0);
        }
        public static async Task<AuthorizationInfo> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var buffer = new byte[ByteLength];
            await connection.ReadAllAsync(buffer, 0, ByteLength);
            return FromBytes(buffer, 0);
        }

        public byte[] AsBytes()
        {
            var bytes = new byte[ByteLength];
            AsBytes(bytes, 0);
            return bytes;
        }
        public void AsBytes(byte[] buffer, int offset)
        {
            UserInfo.AsBytes(buffer, offset);
            Array.Copy(BitConverter.GetBytes(PasswordHash), 0, buffer, offset + UserInfo.ByteLength, sizeof(int));
        }
    }
}
