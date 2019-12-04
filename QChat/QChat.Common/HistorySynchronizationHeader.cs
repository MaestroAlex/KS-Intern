using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct HistorySynchronizationHeader
    {
        public RecieverType RecieverType;
        public int Id;

        public static readonly int ByteLength = sizeof(RecieverType) + sizeof(int);

        public HistorySynchronizationHeader(RecieverType recieverType, int id)
        {
            RecieverType = recieverType;
            Id = id;
        }

        public void AsBytes(byte[] buff, int offset)
        {
            Array.Copy(BitConverter.GetBytes((byte)RecieverType), 0, buff, offset, sizeof(RecieverType));
            offset += sizeof(RecieverType);
            Array.Copy(BitConverter.GetBytes(Id), 0, buff, offset, sizeof(int));
        }
        public byte[] AsBytes()
        {
            var bytes = new byte[ByteLength];
            AsBytes(bytes, 0);
            return bytes;
        }

        public static HistorySynchronizationHeader FromBytes(byte[] bytes, int offset) => new HistorySynchronizationHeader
        {
            RecieverType = (RecieverType)bytes[offset],
            Id = BitConverter.ToInt32(bytes, offset + sizeof(RecieverType))
        };

        public static HistorySynchronizationHeader FromConnection<T>(T connection) where T : IConnection
        {
            var bytes = new byte[ByteLength];
            connection.ReadAll(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
        public static async Task<HistorySynchronizationHeader> FromConnectionAsync<T>(T connection) where T : IConnection
        {
            var bytes = new byte[ByteLength];
            await connection.ReadAllAsync(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
    }
}
