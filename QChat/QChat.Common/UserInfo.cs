using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Common
{
    public struct UserInfo
    {
        public ulong Id;

        public static readonly int ByteLength = sizeof(ulong);       

        public static UserInfo Null { get => new UserInfo() { Id = 0 }; }

        public static UserInfo FromBytes(byte[] buffer, int offset)
        {
            return new UserInfo { Id = BitConverter.ToUInt64(buffer, offset) };
        }

        public byte[] AsBytes() => BitConverter.GetBytes(Id);
        public void AsBytes(byte[] buffer, int offset)
        {
            Array.Copy(BitConverter.GetBytes(Id), 0, buffer, offset, sizeof(ulong));
        }
    }
}
