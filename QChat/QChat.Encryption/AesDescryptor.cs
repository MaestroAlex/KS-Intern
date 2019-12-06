using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace QChat.Encryption
{
    public class AesDescryptor
    {
        private AesCng _aesCng;

        private AesDescryptor(byte[] key)
        {
            _aesCng = new AesCng();
            _aesCng.Key = key;
        }

        public static AesDescryptor Create()
        {
            KeyGenerator.Generate(KeyVariant.Aes128, out var key);

            return new AesDescryptor(key);
        }
        public static AesDescryptor Create(byte[] key)
        {
            switch (key.Length)
            {
                case 128 / 8:
                    return new AesDescryptor(key);
                default:
                    throw new ArgumentException("Invalid key length");
            }
        }


        public byte[] Descrypt(byte[] message)
        {
            var encryptionHeader = EncryptionHeader.FromBytes(message, 0);

            _aesCng.IV = encryptionHeader.IV;
            var descryptor = _aesCng.CreateDecryptor();
            var result = new byte[encryptionHeader.MessageLength];

            using (var memoryStream = new MemoryStream(message, EncryptionHeader.ByteLength, encryptionHeader.MessageLength))
            {
                using (var cryptoStream = new CryptoStream(memoryStream, descryptor, CryptoStreamMode.Read))
                {
                    cryptoStream.Read(result, 0, encryptionHeader.MessageLength);
                }
            }

            return result;
        }
        public async Task<byte[]> DescryptAsync(byte[] message)
        {
            var encryptionHeader = EncryptionHeader.FromBytes(message, 0);

            _aesCng.IV = encryptionHeader.IV;
            var descryptor = _aesCng.CreateDecryptor();
            var result = new byte[encryptionHeader.MessageLength];

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, descryptor, CryptoStreamMode.Read))
                {
                    await cryptoStream.ReadAsync(result, 0, encryptionHeader.MessageLength);
                }
            }

            return result;
        }

        public byte[] DescryptRead<TStream>(TStream stream) where TStream : Stream
        {
            var encryptionHederBytes = new byte[EncryptionHeader.ByteLength];
            stream.Read(encryptionHederBytes, 0, EncryptionHeader.ByteLength);
            
            var encryptionHeader = EncryptionHeader.FromBytes(encryptionHederBytes, 0);

            _aesCng.IV = encryptionHeader.IV;
            var descryptor = _aesCng.CreateDecryptor();
            var result = new byte[encryptionHeader.MessageLength];

            using (var cryptoStream = new CryptoStream(stream, descryptor, CryptoStreamMode.Read))
            {
                cryptoStream.Read(result, 0, encryptionHeader.MessageLength);
            }

            return result;
        }
        public async Task<byte[]> DescryptReadAsync<TStream>(byte[] message, TStream stream) where TStream : Stream
        {
            var encryptionHederBytes = new byte[EncryptionHeader.ByteLength];
            stream.Read(encryptionHederBytes, 0, EncryptionHeader.ByteLength);

            var encryptionHeader = EncryptionHeader.FromBytes(encryptionHederBytes, 0);

            _aesCng.IV = encryptionHeader.IV;
            var descryptor = _aesCng.CreateDecryptor();
            var result = new byte[encryptionHeader.MessageLength];

            using (var cryptoStream = new CryptoStream(stream, descryptor, CryptoStreamMode.Read))
            {
                await cryptoStream.ReadAsync(result, 0, encryptionHeader.MessageLength);
            }

            return result;
        }
    }
}
