using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatHandler
{
    public class Encrypt
    {
        private static readonly byte[] privateKey =
        {
            0x4a , 0x50 , 0xde , 0x2b
          , 0x46 , 0x14 , 0x0a , 0x8a
          , 0x9e , 0x38 , 0xdd , 0xc0
          , 0x98 , 0x4b , 0x4e , 0x8a
          , 0x0a , 0xd3 , 0x5b , 0x27
          , 0xcb , 0x7e , 0x46 , 0xad
          , 0x17 , 0x00 , 0xe5 , 0xd1
          , 0x79 , 0x29 , 0xa0 , 0xc0
        };

        public static async Task<string> EncodeMessage(string message)
        {
            string result = "";
            byte[] data = Encoding.Unicode.GetBytes(message);

            using (var aes = Aes.Create())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(privateKey, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(message);
                        }
                        result = $"{Convert.ToBase64String(aes.IV)}<div>{Convert.ToBase64String(msEncrypt.ToArray())}";
                    }
                }
            }
            return result;
        }

        public static string DecodeMessage(string message)
        {
            string result = "";
            var publicKey = Convert.FromBase64String(message.Substring(0, message.IndexOf("<div>")));
            var cryptedData = Convert.FromBase64String(message.Substring(message.IndexOf("<div>") + "<div>".Length));

            using (Aes aes = Aes.Create())
            {
                aes.Key = privateKey;
                aes.IV = publicKey;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cryptedData))
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
