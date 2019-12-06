using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Encryption;

namespace Testing
{
    class EncryptionTests
    {
        private AesEncryptor _encryptor;
        private AesDescryptor _descryptor;
        private Random _random;


        public EncryptionTests()
        {
            KeyGenerator.Generate(KeyVariant.Aes128, out var key);
            _encryptor = AesEncryptor.Create(key);
            _descryptor = AesDescryptor.Create(key);

            _random = new Random();
        }

        public void Test1()
        {
            var message1 = GetRandomBytes(10);
            OutputBytes(message1);
            var message1encrypted = TestED(message1);
            OutputBytes(message1encrypted);
        }
        public void Test2()
        {
            var message1 = GetRandomBytes(50);
            OutputBytes(message1);
            var message1encrypted = TestED(message1);
            OutputBytes(message1encrypted);
        }
        public void Test3()
        {
            var message1 = GetRandomBytes(100);
            OutputBytes(message1);
            var message1encrypted = TestED(message1);
            OutputBytes(message1encrypted);
        }

        private byte[] TestED(byte[] input)
        {
            var buff = _encryptor.Encrypt(input);
            OutputBytes(buff);
            return _descryptor.Descrypt(buff);
        }

        private byte[] GetRandomBytes(int count)
        {
            var bytes = new byte[count];
            _random.NextBytes(bytes);
            return bytes;
        }

        private void OutputBytes(byte[] buff)
        {
            Console.Write("| ");

            foreach(var b in buff)
            {
                Console.Write($"{b}\t| ");
            }

            Console.WriteLine();
        }
    }
}
