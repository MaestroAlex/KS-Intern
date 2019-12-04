using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct RequestHeader
    {
        public int Version;
        public RequestIntention Intention;

        public static readonly int ByteLength = sizeof(int) + sizeof(RequestIntention);

        public RequestHeader(RequestIntention requestIntention)
        {
            Version = 0x0001;
            Intention = requestIntention;
        }

        public byte[] AsBytes()
        {
            var result = new byte[ByteLength];

            Array.Copy(BitConverter.GetBytes(Version), 0, result, 0, sizeof(int));
            result[sizeof(int)] = (byte)Intention;

            return result;
        }
        public void AsBytes(byte[] buffer, int offset)
        {
            Array.Copy(BitConverter.GetBytes(Version), 0, buffer, offset, sizeof(int));
            buffer[offset + sizeof(int)] = (byte)Intention;
        }
        
        public static RequestHeader FromBytes(byte[] buff, int offset)
        {
            if (buff.Length - offset < ByteLength) throw new ArgumentException();

            return new RequestHeader() { Version = BitConverter.ToInt32(buff, offset), Intention = (RequestIntention)buff[offset + sizeof(int)] };
        }

        public static RequestHeader FromConnection<T>(T connection) where T : IConnectionStream
        {
            var buff = new byte[ByteLength];

            connection.ReadAll(buff, 0, ByteLength);

            return FromBytes(buff, 0);
        }
        public static async Task<RequestHeader> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var buff = new byte[ByteLength];
            await connection.ReadAllAsync(buff, 0, ByteLength);

            return FromBytes(buff, 0);
        }        
    }

    public enum RequestIntention : byte
    {
        Authorization = 0,
        Messaging,
        Registration,
        Disconecting,
        Rooming,  
        HistorySynchronization,
        ConnectionCheck
    }
}
