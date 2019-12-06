using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Encryption
{
    public struct EncryptionHeader
    {
        public byte[] IV;
        public int MessageLength;


        public static readonly int ByteLength = 128 / 8 + sizeof(int);


        public byte[] AsBytes()
        {
            var result = new byte[ByteLength];
            AsBytes(result, 0);
            return result;
        } 
        public void AsBytes(byte[] buffer, int offset)
        {
            Array.Copy(IV, 0, buffer, 0, 128 / 8);
            Array.Copy(BitConverter.GetBytes(MessageLength), 0, buffer, 128 / 8, sizeof(int));
        }

        public static EncryptionHeader FromBytes(byte[] buff, int offset)
        {
            var ivBytes = new byte[128 / 8];
            Array.Copy(buff, 0, ivBytes, 0, 128 / 8);

            var messageLength = BitConverter.ToInt32(buff, 128 / 8);

            return new EncryptionHeader
            {
                IV = ivBytes,
                MessageLength = messageLength
            };
        }
    }
}
