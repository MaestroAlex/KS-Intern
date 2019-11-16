using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Common
{
    public struct SenderInfo
    {
        public ulong Id;

        public static readonly int ByteLength = sizeof(ulong);

        public byte[] AsBytes() => BitConverter.GetBytes(Id);
        public void AsBytes(byte[] buffer, int offset)
        {
            var bytes = BitConverter.GetBytes(Id);
            Array.Copy(bytes, 0, buffer, offset, ByteLength);
        }

        public static SenderInfo FromBytes(byte[] buff, int offset) => new SenderInfo()
        {
            Id = BitConverter.ToUInt64(buff, offset)
        };
    }
}
