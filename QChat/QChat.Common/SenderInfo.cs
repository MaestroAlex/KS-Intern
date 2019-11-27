using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Common
{
    public struct SenderInfo
    {
        public UserInfo UserInfo;

        public static readonly int ByteLength = sizeof(ulong);

        public byte[] AsBytes() => UserInfo.AsBytes();
        public void AsBytes(byte[] buffer, int offset)
        {
            UserInfo.AsBytes(buffer, offset);
        }

        public static SenderInfo FromBytes(byte[] buff, int offset) => new SenderInfo()
        {
            UserInfo = UserInfo.FromBytes(buff, offset)
        };
    }
}
