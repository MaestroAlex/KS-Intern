using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RegistrationResponceInfo
    {
        public bool Success;
        public UserInfo UserInfo;

        public static int ByteLength = sizeof(bool) + sizeof(ulong);

        public void AsBytes(byte[] buff, int offset)
        {
            Array.Copy(BitConverter.GetBytes(Success), 0, buff, offset, sizeof(bool));
            offset += sizeof(bool);
            UserInfo.AsBytes(buff, offset);
        }
        public byte[] AsBytes()
        {
            var bytes = new byte[ByteLength];
            AsBytes(bytes, 0);
            return bytes;
        }

        public static RegistrationResponceInfo FromBytes(byte[] buff, int offset) => new RegistrationResponceInfo
        {
            Success = BitConverter.ToBoolean(buff, offset),
            UserInfo = UserInfo.FromBytes(buff, offset + sizeof(bool))
        };

        public static RegistrationResponceInfo FromConnection<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            if (connection.Read(bytes, 0, ByteLength) <= 0) throw new Exception();
            return FromBytes(bytes, 0);
        }
        public static async Task<RegistrationResponceInfo> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            if (await connection.ReadAsync(bytes, 0, ByteLength) <= 0) throw new Exception();
            return FromBytes(bytes, 0);
        }
    }
}
