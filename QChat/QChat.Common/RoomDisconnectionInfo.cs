using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RoomDisconnectionInfo
    {
        public RoomInfo RoomInfo;

        public static readonly int ByteLength = RoomInfo.ByteLength;

        public RoomDisconnectionInfo(RoomInfo info)
        {
            RoomInfo = info;
        }

        public void AsBytes(byte[] buff, int offset)
        {
            RoomInfo.AsBytes(buff, offset);
        }
        public byte[] AsBytes() => RoomInfo.AsBytes();

        public static RoomDisconnectionInfo FromBytes(byte[] bytes, int offset) => new RoomDisconnectionInfo
        {
            RoomInfo = RoomInfo.FromBytes(bytes, offset)
        };

        public static RoomDisconnectionInfo FromConnection<T>(T connetction) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            connetction.ReadAll(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
        public static async Task<RoomDisconnectionInfo> FromConnectionAsync<T>(T connetction) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            await connetction.ReadAllAsync(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
    }
}
