using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Common
{
    public struct Content
    {
        private byte[] _buff;

        public int Length { get => _buff.Length; }

        public byte[] AsBytes() => _buff;
        public void AsBytes(byte[] buffer, int offset) => Array.Copy(_buff, 0, buffer, offset, _buff.Length);

        static public Content Wrap(byte[] src)
        {
            return new Content { _buff = src };
        }

        static public Content Copy(byte[] src)
        {
            var result = new Content { _buff = new byte[src.Length] };
            src.CopyTo(result._buff, 0);

            return result;
        }
    }
}
