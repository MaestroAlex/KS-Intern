using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct ResponceHeader
    {
        public int Version;
        public ResponceIntention Intention;

        public static int ByteLength = sizeof(int) + sizeof(ResponceIntention);

        public ResponceHeader(ResponceIntention intention)
        {
            Version = 0x0001;
            Intention = intention;
        }


        public byte[] AsBytes()
        {
            var result = new byte[ByteLength];

            AsBytes(result, 0);

            return result;
        }
        public void AsBytes(byte[] buffer, int offset)
        {
            Array.Copy(BitConverter.GetBytes(Version), 0, buffer, offset, sizeof(int));
            buffer[offset + sizeof(int)] = (byte)Intention;
        }

        public static ResponceHeader FromBytes(byte[] buff) => FromBytes(buff, 0);
        public static ResponceHeader FromBytes(byte[] buff, int offset) => new ResponceHeader
        {
            Version = BitConverter.ToInt32(buff, offset),
            Intention = (ResponceIntention)buff[offset + sizeof(int)]
        };


        public static ResponceHeader FromConnection<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            if (connection.Read(bytes, 0, ByteLength) <= 0) throw new Exception();
            return FromBytes(bytes);
        }
        public static async Task<ResponceHeader> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[ByteLength];
            if (await connection.ReadAsync(bytes, 0, ByteLength) <= 0) throw new Exception();
            return FromBytes(bytes);
        }
    }

    public enum ResponceIntention : byte
    {
        Authorization = 0,
        Messaging,
        Registration,
        Disconecting,
        Rooming,
    }
}
