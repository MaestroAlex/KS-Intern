using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RegistrationResponce
    {
        public RegistrationResult Result;
        public UserInfo UserInfo;

        public static readonly int ByteLength = sizeof(RegistrationResult) + sizeof(ulong);

        public void AsBytes(byte[] buff, int offset)
        {
            Array.Copy(BitConverter.GetBytes((byte)Result), 0, buff, offset, sizeof(bool));
            offset += sizeof(RegistrationResult);
            UserInfo.AsBytes(buff, offset);
        }
        public byte[] AsBytes()
        {
            var bytes = new byte[ByteLength];
            AsBytes(bytes, 0);
            return bytes;
        }

        public static RegistrationResponce FromBytes(byte[] buff, int offset) => new RegistrationResponce
        {
            Result = (RegistrationResult)buff[offset],
            UserInfo = UserInfo.FromBytes(buff, offset + sizeof(RegistrationResult))
        };

        public static RegistrationResponce FromConnection<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            connection.ReadAll(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
        public static async Task<RegistrationResponce> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            await connection.ReadAllAsync(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
    }

    public enum RegistrationResult : byte
    {
        Success,
        NicknameAlreadyRegistered,
        Fail
    }
}
