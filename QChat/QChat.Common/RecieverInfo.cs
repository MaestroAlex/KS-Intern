using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Common
{
    public struct RecieverInfo
    {
        public RecieverType Type;
        public int Id;

        public static readonly int ByteLength = sizeof(RecieverType) + sizeof(int);

        public byte[] AsBytes()
        {
            var result = new byte[ByteLength];
            result[0] = (byte)Type;
            Array.Copy(BitConverter.GetBytes(Id), 0, result, sizeof(RecieverType), sizeof(int));

            return result;
        }
        public void AsBytes(byte[] buffer, int offset)
        {
            buffer[offset] = (byte)Type;
            Array.Copy(BitConverter.GetBytes(Id), 0, buffer, offset + sizeof(RecieverType), sizeof(int));
        }

        public static RecieverInfo FromBytes(byte[] buff, int offset)
        {
            return new RecieverInfo()
            {
                Type = (RecieverType)buff[offset],
                Id = BitConverter.ToInt32(buff, offset + sizeof(RecieverType))
            };
        }
    }
    public enum RecieverType : byte
    {
        None    = 0x0,
        User    = 0x1,
        Group   = 0x2,
        Room    = 0x3,
    }
}
