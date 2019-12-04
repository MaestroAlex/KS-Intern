using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RoomSynchronizationInfo
    {
        public bool IsPublic;
        public int Id;
        public NetString Name;

        public int ByteLength => sizeof(bool) + sizeof(int) + Name.ByteLength;

        public RoomSynchronizationInfo(bool isPublic, int id, string name)
        {
            IsPublic = isPublic;
            Id = id;
            Name = new NetString(name);
        }

        public void AsBytes(byte[] buff, int offset)
        {
            Array.Copy(BitConverter.GetBytes(IsPublic), 0, buff, offset, sizeof(bool));
            offset += sizeof(bool);
            Array.Copy(BitConverter.GetBytes(Id), 0, buff, offset, sizeof(int));
            offset += sizeof(int);
            Name.AsBytes(buff, offset);
        }
        public byte[] AsBytes()
        {
            var bytes = new byte[ByteLength];
            AsBytes(bytes, 0);
            return bytes;
        }

        public static RoomSynchronizationInfo FromBytes(byte[] buff, int offset)
        {
            var result = new RoomSynchronizationInfo();

            result.IsPublic = BitConverter.ToBoolean(buff, offset);
            offset += sizeof(bool);
            result.Id = BitConverter.ToInt32(buff, offset);
            offset += sizeof(int);
            result.Name = NetString.FromBytes(buff, offset);

            return result;
        }

        public static RoomSynchronizationInfo FromConnection<T>(T connection) where T: IConnectionStream
        {
            var firstBytes = new byte[sizeof(int) + sizeof(bool)];
            connection.ReadAll(firstBytes, 0, sizeof(bool) + sizeof(int));
            var name = NetString.FromConnection(connection);

            return new RoomSynchronizationInfo
            {
                IsPublic = BitConverter.ToBoolean(firstBytes, 0),
                Id = BitConverter.ToInt32(firstBytes, sizeof(bool)),
                Name = name
            };
        }
        public static async Task<RoomSynchronizationInfo> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var firstBytes = new byte[sizeof(int) + sizeof(bool)];
            await connection.ReadAllAsync(firstBytes, 0, sizeof(bool) + sizeof(int));
            var name = await NetString.FromConnectionAsync(connection);

            return new RoomSynchronizationInfo
            {
                IsPublic = BitConverter.ToBoolean(firstBytes, 0),
                Id = BitConverter.ToInt32(firstBytes, sizeof(bool)),
                Name = name
            };
        }
    }
}
