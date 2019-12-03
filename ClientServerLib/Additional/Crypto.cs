using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using ClientServerLib.Common;

namespace ClientServerLib.Additional
{
    internal class Crypto
    {
        private static readonly byte[] PrivateKey =
        {
            0x00, 0x22, 0x11, 0x11, 0x26, 0x47, 0x23, 0xb3,
            0x00, 0x01, 0x11, 0x11, 0x55, 0x12, 0x23, 0xb1,
            0xdd, 0xa2, 0x11, 0x11, 0xb7, 0x31, 0xa1, 0xb2,
            0x00, 0x33, 0x11, 0xf1, 0x21, 0xd3, 0x55, 0xb1
        };

        public static async Task<string> Encrypt(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new Exception("Source message is empty!");

            string encrypted;
            using (Aes aes = Aes.Create())
            {
                aes.Key = PrivateKey;
                aes.GenerateIV();

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(message);
                        }
                        encrypted = Convert.ToBase64String(aes.IV) + ChatSyntax.MessageDiv + Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            return encrypted;
        }

        public static async Task<string> Decrypt(string ecnryptedMessage)
        {
            if (string.IsNullOrEmpty(ecnryptedMessage))
                throw new Exception("Source message is empty!");

            string decrypted;
            using (Aes aesAlg = Aes.Create())
            {
                string[] KeyMessage = ecnryptedMessage.Split(new string[] { ChatSyntax.MessageDiv }, StringSplitOptions.None);
                if (KeyMessage.Length != 2)
                {
                    throw new Exception("Can't split message into key and message!");
                }
                aesAlg.IV = Convert.FromBase64String(KeyMessage[0]);
                aesAlg.Key = PrivateKey;
                byte[] bytesToDecrypt = Convert.FromBase64String(KeyMessage[1]);
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(bytesToDecrypt))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            decrypted = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return decrypted;

        }


        public static string GetSha256(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }
    }
}
