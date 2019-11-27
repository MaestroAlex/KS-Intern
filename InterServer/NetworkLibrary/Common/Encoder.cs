using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLibrary.Common
{
    public class Encoder
    {
        private static readonly byte[] EncodeKey =
        {
            0x00, 0x22, 0x11, 0x11, 0x26, 0x47, 0x23, 0xb3,
            0x00, 0x01, 0x11, 0x11, 0x55, 0x12, 0x23, 0xb1,
            0xdd, 0xa2, 0x11, 0x11, 0xb7, 0x31, 0xa1, 0xb2,
            0x00, 0x33, 0x11, 0xf1, 0x21, 0xd3, 0x55, 0xb1
        };

        public static async Task<string> EncodeMessage(string message)
        {
            string result = "";
            var rnd = new Random();
            byte[] data = Encoding.ASCII.GetBytes(message);

            using (var aes = AesCng.Create())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(EncodeKey, aes.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(message);
                        }
                        result = Convert.ToBase64String(aes.IV) + "<div>" + Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            return result;
        }

        public static async Task<string> DecodeMessage(string message)
        {
            string result = "";
            var IV = Convert.FromBase64String(message.Substring(0, message.IndexOf("<div>")));
            var cipherText = Convert.FromBase64String(message.Substring(message.IndexOf("<div>") + 5));
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = EncodeKey;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            result = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return result;
        }
    }
}
