using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RoomingResponceHeader
    {
        public RoomingResponceIntention Intention;

        public static readonly int ByteLength = sizeof(RoomingResponceIntention);


        public RoomingResponceHeader(RoomingResponceIntention intention)
        {
            Intention = intention;
        }


        public void AsBytes(byte[] buff, int offset)
        {
            buff[offset] = (byte)Intention;
        }
        public byte[] AsBytes() => new byte[]
        {
            (byte)Intention
        };

        public static RoomingResponceHeader FromBytes(byte[] bytes, int offset) => new RoomingResponceHeader
        {
            Intention = (RoomingResponceIntention)bytes[offset]
        };

        public static RoomingResponceHeader FromConnection<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            connection.ReadAll(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
        public static async Task <RoomingResponceHeader> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            await connection.ReadAllAsync(bytes, 0, ByteLength);
            return FromBytes(bytes, 0);
        }
    }

    public enum RoomingResponceIntention : byte
    {
        InvitationNotification
    }
}
