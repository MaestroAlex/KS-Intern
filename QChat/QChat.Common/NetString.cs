using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Common
{
    public struct NetString
    {
        private string _value;

        public string Value => _value;

        public int ByteLength => sizeof(int) + Encoding.Unicode.GetByteCount(_value);

        public NetString(string value)
        {
            _value = value;
        }

        public void AsBytes(byte[] buff, int offset)
        {
            var _nameBytes = Encoding.Unicode.GetBytes(_value);
            Array.Copy(BitConverter.GetBytes(_nameBytes.Length), 0, buff, offset, sizeof(int));
            offset += sizeof(int);
            Array.Copy(_nameBytes, 0, buff, offset, _nameBytes.Length);
        }
        public byte[] AsBytes()
        {
            var bytes = new byte[ByteLength];
            AsBytes(bytes, 0);
            return bytes;
        }

        public static NetString FromBytes(byte[] buff, int offset)
        {
            var length = BitConverter.ToInt32(buff, offset);
            offset += sizeof(int);
            return new NetString
            {
                _value = Encoding.Unicode.GetString(buff, offset, length)
            };
        }

        public static NetString FromConnection<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[sizeof(int)];
            connection.ReadAll(bytes, 0, sizeof(int));

            var length = BitConverter.ToInt32(bytes, 0);
            Array.Resize(ref bytes, length);

            connection.ReadAll(bytes, 0, length);
            return new NetString(Encoding.Unicode.GetString(bytes, 0, length));
        }
        public static async Task<NetString> FromConnectionAsync<T>(T connection) where T : IConnectionStream
        {
            var bytes = new byte[sizeof(int)];
            await connection.ReadAllAsync(bytes, 0, sizeof(int));

            var length = BitConverter.ToInt32(bytes, 0);
            Array.Resize(ref bytes, length);

            await connection.ReadAllAsync(bytes, 0, length);
            return new NetString(Encoding.Unicode.GetString(bytes, 0, length));
        }
    }
}
