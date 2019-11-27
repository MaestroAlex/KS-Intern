using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct MessageHeader 
    {
        public ContentType ContentType;
        public SenderInfo SenderInfo;
        public RecieverInfo RecieverInfo;
        public int Length;

        public static readonly int ByteLength = sizeof(ContentType) + SenderInfo.ByteLength + RecieverInfo.ByteLength + sizeof(int);

        public static MessageHeader FromBytes(byte[] buffer, int offset)
        {
            if (buffer.Length - offset < ByteLength) throw new ArgumentException();

            return new MessageHeader()
            {
                ContentType = (ContentType)buffer[offset],
                SenderInfo = SenderInfo.FromBytes(buffer, offset + sizeof(ContentType)),
                RecieverInfo = RecieverInfo.FromBytes(buffer, offset + sizeof(ContentType) + SenderInfo.ByteLength),
                Length = BitConverter.ToInt32(buffer, offset + sizeof(ContentType) + SenderInfo.ByteLength + RecieverInfo.ByteLength)
            };
        }

        public static MessageHeader FromConnection<C>(C connection) where C : IConnectionStream
        {
            var buffer = new byte[ByteLength];
            if (connection.Read(buffer, 0, ByteLength) <= 0) throw new Exception();

            return FromBytes(buffer, 0);
        }
        public static async Task<MessageHeader> FromConnectionAsync<C>(C connection) where C : IConnectionStream
        {
            var buffer = new byte[ByteLength];
            if (await connection.ReadAsync(buffer, 0, ByteLength) <= 0) throw new Exception();

            return FromBytes(buffer, 0);
        }

        public byte[] AsBytes()
        {
            var bytes = new byte[ByteLength];
            AsBytes(bytes, 0);
            return bytes;
        }
        public void AsBytes(byte[] buffer, int offset)
        {
            buffer[offset] = (byte)ContentType; offset += sizeof(ContentType);
            SenderInfo.AsBytes(buffer, offset); offset += SenderInfo.ByteLength;
            RecieverInfo.AsBytes(buffer, offset); offset += RecieverInfo.ByteLength;
            Array.Copy(BitConverter.GetBytes(Length), 0, buffer, offset, sizeof(int));
        }
    }
}
