using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace QChat.Encryption
{
    public class AesEncryptor
    {
        private AesCng _aesCng;

        private AesEncryptor(byte[] key)
        {
            _aesCng = new AesCng();
            _aesCng.Key = key;
        }    
        
        public static AesEncryptor Create()
        {
            KeyGenerator.Generate(KeyVariant.Aes128, out var key);

            return new AesEncryptor(key);
        }
        public static AesEncryptor Create(byte[] key)
        {
            switch (key.Length)
            {
                case 128 / 8:
                    return new AesEncryptor(key);
                default:
                    throw new ArgumentException("Invalid key length");
            }
        }


        public byte[] Encrypt(byte[] message)
        {
            KeyGenerator.Generate(KeyVariant.Aes128, out var key);
            _aesCng.IV = key;
            var encryptor = _aesCng.CreateEncryptor();

            var messageLength = message.Length;

            if (messageLength != 0)
            {
                var blockSize = _aesCng.BlockSize / 8;
                messageLength = ((messageLength - 1) / blockSize + 1) * blockSize;
            }            

            var encryptionHeader = new EncryptionHeader
            {
                IV = key,
                MessageLength = messageLength
            };            

            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(encryptionHeader.AsBytes(), 0, EncryptionHeader.ByteLength);

                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(message, 0, message.Length);                    
                }

                return memoryStream.ToArray();
            }
        }
        public async Task<byte[]> EncryptAsync(byte[] message)
        {
            KeyGenerator.Generate(KeyVariant.Aes128, out var key);
            _aesCng.IV = key;
            var encryptor = _aesCng.CreateEncryptor();

            var messageLength = message.Length;

            if (messageLength != 0)
            {
                var blockSize = _aesCng.BlockSize;
                messageLength = ((messageLength - 1) / blockSize + 1) * blockSize;
            }

            var encryptionHeader = new EncryptionHeader
            {
                IV = key,
                MessageLength = messageLength
            };

            using (var memoryStream = new MemoryStream())
            {
                await memoryStream.WriteAsync(encryptionHeader.AsBytes(), 0, EncryptionHeader.ByteLength);

                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    await cryptoStream.WriteAsync(message, 0, message.Length);
                }

                return memoryStream.ToArray();
            }
        }

        public byte[] Encrypt(IEnumerable<byte[]> messagesCollection)
        {
            KeyGenerator.Generate(KeyVariant.Aes128, out var key);
            _aesCng.IV = key;
            var encryptor = _aesCng.CreateEncryptor();

            var messageLength = messagesCollection.Sum(arr => arr.Length);

            if (messageLength != 0)
            {
                var blockSize = _aesCng.BlockSize / 8;
                messageLength = ((messageLength - 1) / blockSize + 1) * blockSize;
            }

            var encryptionHeader = new EncryptionHeader
            {
                IV = key,
                MessageLength = messageLength
            };

            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(encryptionHeader.AsBytes(), 0, EncryptionHeader.ByteLength);

                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    foreach(var message in messagesCollection)
                    {
                        cryptoStream.Write(message, 0, message.Length);
                    }
                }

                return memoryStream.ToArray();
            }
        }

        public void EncryptWrite<TStream>(byte[] message, TStream stream) where TStream : Stream
        {
            KeyGenerator.Generate(KeyVariant.Aes128, out var key);
            _aesCng.IV = key;
            var encryptor = _aesCng.CreateEncryptor();

            var messageLength = message.Length;

            if (messageLength != 0)
            {
                var blockSize = _aesCng.BlockSize;
                messageLength = ((messageLength - 1) / blockSize + 1) * blockSize;
            }

            var encryptionHeader = new EncryptionHeader
            {
                IV = key,
                MessageLength = messageLength
            };

            stream.Write(encryptionHeader.AsBytes(), 0, EncryptionHeader.ByteLength);

            using (var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(message, 0, message.Length);
            }            
        }
        public async Task EncryptWriteAsync<TStream>(byte[] message, TStream stream) where TStream : Stream
        {
            KeyGenerator.Generate(KeyVariant.Aes128, out var key);
            _aesCng.IV = key;
            var encryptor = _aesCng.CreateEncryptor();

            var messageLength = message.Length;

            if (messageLength != 0)
            {
                var blockSize = _aesCng.BlockSize;
                messageLength = ((messageLength - 1) / blockSize + 1) * blockSize;
            }

            var encryptionHeader = new EncryptionHeader
            {
                IV = key,
                MessageLength = messageLength
            };

            await stream.WriteAsync(encryptionHeader.AsBytes(), 0, EncryptionHeader.ByteLength);

            using (var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
            {
                await cryptoStream.WriteAsync(message, 0, message.Length);
            }
        }
    }
}
