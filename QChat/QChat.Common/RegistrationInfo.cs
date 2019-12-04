using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RegistrationInfo
    {
        public int LoginLength { get => Login.Length; }
        public string Login;
        public int PasswordHash;

        public void AsBytes(byte[] buffer, int offset)
        {
            Array.Copy(BitConverter.GetBytes(Encoding.Unicode.GetByteCount(Login)), 0, buffer, offset, sizeof(int));

            offset += sizeof(int);
            Encoding.Unicode.GetBytes(Login, 0, LoginLength, buffer, offset);

            offset += Encoding.Unicode.GetByteCount(Login);
            Array.Copy(BitConverter.GetBytes(PasswordHash), 0, buffer, offset, sizeof(int));
        }
        public byte[] AsBytes()
        {
            var bytes = new byte[sizeof(int) + Encoding.Unicode.GetByteCount(Login) + sizeof(int)];
            AsBytes(bytes, 0);
            return bytes;
        }

        public static RegistrationInfo FromBytes(byte[] buff, int offset)
        {
            var loginLength = BitConverter.ToInt32(buff, offset);

            offset += sizeof(int);
            var login = Encoding.Unicode.GetString(buff, offset, loginLength);

            offset += loginLength;
            var passwordHash = BitConverter.ToInt32(buff, offset);

            return new RegistrationInfo
            {
                Login = login,
                PasswordHash = passwordHash
            };
        }
        
        public static RegistrationInfo FromConnection<T>(T connection) where T : IConnectionStream
        {
            var buffer = new byte[sizeof(int)];

            connection.ReadAll(buffer, 0, sizeof(int));

            var loginLength = BitConverter.ToInt32(buffer, 0);
            var loginBuffer = new byte[loginLength + sizeof(int)];

            connection.ReadAll(loginBuffer, 0, loginLength + sizeof(int));
            var login = Encoding.Unicode.GetString(loginBuffer, 0, loginLength);
            var passwordHash = BitConverter.ToInt32(loginBuffer, loginLength);

            return new RegistrationInfo
            {
                Login = login,
                PasswordHash = passwordHash
            };
        }
        public static async Task<RegistrationInfo> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var buffer = new byte[sizeof(int)];

            await connection.ReadAllAsync(buffer, 0, sizeof(int));

            var loginLength = BitConverter.ToInt32(buffer, 0);
            var loginBuffer = new byte[loginLength + sizeof(int)];

            await connection.ReadAllAsync(loginBuffer, 0, loginLength + sizeof(int));
            var login = Encoding.Unicode.GetString(loginBuffer, 0, loginLength);
            var passwordHash = BitConverter.ToInt32(loginBuffer, loginLength);

            return new RegistrationInfo
            {
                Login = login,
                PasswordHash = passwordHash
            };
        }
    }
}
