using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RoomingHeader
    {
        public RoomingIntention Intention;
        public RoomInfo Info;

        public static readonly int ByteLength = sizeof(RoomingIntention) + RoomInfo.ByteLength;  

        public byte[] AsBytes()
        {
            var bytes = new byte[ByteLength];
            AsBytes(bytes, 0);
            return bytes;
        }
        public void AsBytes(byte[] buffer, int offset)
        {
            buffer[offset++] = (byte)Intention;
            Info.AsBytes(buffer, offset);
        }

        public static RoomingHeader FromBytes(byte[] buffer, int offset) => new RoomingHeader
        {
            Intention = (RoomingIntention)buffer[offset],
            Info = RoomInfo.FromBytes(buffer, sizeof(RoomingIntention))
        };
        

        public static RoomingHeader FromConnection<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            connection.Read(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
        public static async Task<RoomingHeader> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            await connection.ReadAsync(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
    }

    public enum RoomingIntention
    {
        Create = 0,
        Join,
        Connect,
        Remove
    }
}
