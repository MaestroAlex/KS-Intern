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

        public static readonly int ByteLength = sizeof(RoomingIntention);  


        public RoomingHeader(RoomingIntention intention)
        {
            Intention = intention;
        }

        public byte[] AsBytes()
        {
            var bytes = new byte[ByteLength];
            AsBytes(bytes, 0);
            return bytes;
        }
        public void AsBytes(byte[] buffer, int offset)
        {
            buffer[offset] = (byte)Intention;
        }

        public static RoomingHeader FromBytes(byte[] buffer, int offset) => new RoomingHeader
        {
            Intention = (RoomingIntention)buffer[offset]
        };
        

        public static RoomingHeader FromConnection<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            connection.ReadAll(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
        public static async Task<RoomingHeader> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            await connection.ReadAllAsync(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
    }

    public enum RoomingIntention : byte
    {
        Create = 0,
        Join,
        Connect,
        Disconnect,
        Remove,
        Invitation,
        Synchronization
    }
}
