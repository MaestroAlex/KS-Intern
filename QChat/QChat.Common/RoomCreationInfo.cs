using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RoomCreationInfo
    {
        public bool IsPublic;
        public string Name;
        public int NameLength { get => Name.Length; }

        public void AsBytes(byte[] buffer, int offset)
        {
            Array.Copy(BitConverter.GetBytes(IsPublic), 0, buffer, offset, sizeof(bool));

            offset += sizeof(bool);
            Array.Copy(BitConverter.GetBytes(NameLength), 0, buffer, offset, sizeof(int));

            var bytes = Encoding.Unicode.GetBytes(Name);
            offset += sizeof(int);
            Array.Copy(bytes, 0, buffer, offset, bytes.Length);
        }
        public byte[] AsBytes()
        {
            var result = new byte[sizeof(bool) + sizeof(int) + NameLength];
            AsBytes(result, 0);
            return result;
        }

        public static RoomCreationInfo FromBytes(byte[] buffer, int offset)
        {
            var isPublic = BitConverter.ToBoolean(buffer, offset);

            offset += sizeof(bool);
            var nameLength = BitConverter.ToInt32(buffer, offset);

            offset += sizeof(int);

            return new RoomCreationInfo
            {
                Name = Encoding.Unicode.GetString(buffer, offset, nameLength),
                IsPublic = isPublic
            };            
        }

        public static RoomCreationInfo FromConnection<T>(T connection) where T : IConnectionStream
        {
            var buffer = new byte[sizeof(bool) + sizeof(int)];

            if (connection.Read(buffer, 0, sizeof(bool) + sizeof(int)) <= 0) throw new Exception();
            var isPublic = BitConverter.ToBoolean(buffer, 0);
            var nameLength = BitConverter.ToInt32(buffer, sizeof(bool));

            Array.Resize(ref buffer, nameLength);

            if (connection.Read(buffer, 0, nameLength) <= 0) throw new Exception();
            var name = Encoding.Unicode.GetString(buffer);

            return new RoomCreationInfo
            {
                IsPublic = isPublic,
                Name = name
            };
        }
        public static async Task<RoomCreationInfo> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var buffer = new byte[sizeof(bool) + sizeof(int)];

            if (await connection.ReadAsync(buffer, 0, sizeof(bool) + sizeof(int)) <= 0) throw new Exception();

            var isPublic = BitConverter.ToBoolean(buffer, 0);
            var nameLength = BitConverter.ToInt32(buffer, sizeof(bool));

            Array.Resize(ref buffer, nameLength);

            if (await connection.ReadAsync(buffer, 0, nameLength) <= 0) throw new Exception();
            var name = Encoding.Unicode.GetString(buffer);

            return new RoomCreationInfo
            {
                IsPublic = isPublic,
                Name = name
            };
        }
    }
}
