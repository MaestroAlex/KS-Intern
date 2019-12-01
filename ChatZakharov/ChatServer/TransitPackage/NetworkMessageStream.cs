using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TransitPackage
{
    public class NetworkMessageStream : IDisposable
    {
        public Aes Aes { get; private set; }

        private NetworkStream stream;
        private BinaryWriter networkWriter;
        private BinaryReader networkReader;

        public NetworkMessageStream(NetworkStream stream)
        {
            this.stream = stream;
            networkWriter = new BinaryWriter(stream);
            networkReader = new BinaryReader(stream);
            Aes = Aes.Create();
        }

        public async Task WriteAsync(NetworkMessage message)
        {
            byte[] byteArrayMessage = message.Serialize();
            byte[] resData = new byte[5 + byteArrayMessage.Length]; // 4 - obj size, 1 - encryption flag (true - encrypted)
            BitConverter.GetBytes(false).CopyTo(resData, 0);
            BitConverter.GetBytes(byteArrayMessage.Length).CopyTo(resData, 1);
            byteArrayMessage.CopyTo(resData, 5);
            await stream.WriteAsync(resData, 0, resData.Length);
        }

        public async Task WriteEncryptedAsync(NetworkMessage message)
        {
            if (Aes.Key == null || Aes.Key.Length <= 0)
                throw new ArgumentNullException("Key");

            byte[] IV = GenerateIV();

            using (ICryptoTransform encryptor = Aes.CreateEncryptor(Aes.Key, IV))
            {
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] serializedMessage = message.Serialize();
                        int decryptedMessageSize = serializedMessage.Length;

                        await csEncrypt.WriteAsync(serializedMessage, 0, serializedMessage.Length);
                        csEncrypt.FlushFinalBlock();

                        NetworkMessageEncrypted encryptedMessage =
                            new NetworkMessageEncrypted(msEncrypt.ToArray(), IV, decryptedMessageSize);

                        byte[] byteArrayEncryptedMessage = encryptedMessage.Serialize();
                        byte[] resDataEncrypted = new byte[5 + byteArrayEncryptedMessage.Length]; // 4 - obj size, 1 - encryption flag (true - encrypted)

                        BitConverter.GetBytes(true).CopyTo(resDataEncrypted, 0);
                        BitConverter.GetBytes(byteArrayEncryptedMessage.Length).CopyTo(resDataEncrypted, 1);
                        byteArrayEncryptedMessage.CopyTo(resDataEncrypted, 5);
                        await stream.WriteAsync(resDataEncrypted, 0, resDataEncrypted.Length);

                        #region logging
                        //using (StreamWriter writer = new StreamWriter(File.Open(@"..\..\..\..\crypto_write_log.txt", FileMode.Create)))
                        //{
                        //    writer.WriteLine("write log");

                        //    writer.WriteLine($"AES Key = size: {Aes.Key.Length}");
                        //    for (int i = 0; i < Aes.Key.Length; i++)
                        //        writer.Write(Aes.Key[i] + " ");
                        //    writer.WriteLine("");

                        //    writer.WriteLine($"AES IV = size: {IV.Length}");
                        //    for (int i = 0; i < IV.Length; i++)
                        //        writer.Write(IV[i] + " ");
                        //    writer.WriteLine("");

                        //    writer.WriteLine($"Encrypted = size {encryptedMessage.EncryptedNetworkMessage.Length}");
                        //    for (int i = 0; i < encryptedMessage.EncryptedNetworkMessage.Length; i++)
                        //        writer.Write(encryptedMessage.EncryptedNetworkMessage[i] + " ");
                        //    writer.WriteLine("");

                        //    writer.WriteLine($"Decrypted = size {serializedMessage.Length}");
                        //    for (int i = 0; i < serializedMessage.Length; i++)
                        //        writer.Write(serializedMessage[i] + " ");
                        //}
                        #endregion
                    }
                }
            }
        }

        private byte[] GenerateIV()
        {
            Random rand = new Random();
            byte[] res = new byte[16];
            for (int i = 0; i < res.Length; i++)
                res[i] = (byte)rand.Next(0, 255);

            return res;
        }

        public async Task<NetworkMessage> ReadAsync()
        {
            // networkStream не знает будет отправлено зашифрованное сообщение или нет => Read один, в нём определям
            // зашифрованное оно или нет с помощью флага
            byte[] buffer = new byte[1];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            bool isEncrypted = BitConverter.ToBoolean(buffer, 0);
            return isEncrypted ? await InternalReadEncrypted() : await InternalRead();
        }

        private async Task<NetworkMessage> InternalRead()
        {
            byte[] dataSizeRaw = new byte[4];
            await stream.ReadAsync(dataSizeRaw, 0, dataSizeRaw.Length);
            int dataSize = BitConverter.ToInt32(dataSizeRaw, 0);

            byte[] readData = await InternalReadStreamAsync(dataSize);

            return NetworkMessage.Deserialize(readData);
            //return NetworkMessage.Deserialize(networkReader.ReadBytes(dataSize));
        }

        private async Task<NetworkMessage> InternalReadEncrypted()
        {
            if (Aes.Key == null || Aes.Key.Length <= 0)
                throw new ArgumentNullException("Key");

            byte[] dataSizeRaw = new byte[4];
            await stream.ReadAsync(dataSizeRaw, 0, dataSizeRaw.Length);
            int encryptedDataSize = BitConverter.ToInt32(dataSizeRaw, 0);

            NetworkMessageEncrypted encryptedMessage =
                NetworkMessageEncrypted.Deserialize(await InternalReadStreamAsync(encryptedDataSize));

            if (encryptedMessage.IV == null || encryptedMessage.IV.Length <= 0)
                throw new ArgumentNullException("IV");

            NetworkMessage networkMessage = null;

            try
            {
                using (ICryptoTransform decryptor = Aes.CreateDecryptor(Aes.Key, encryptedMessage.IV))
                {
                    using (MemoryStream msDecrypt = new MemoryStream(encryptedMessage.EncryptedNetworkMessage))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            byte[] buffer = new byte[4096];
                            int curRead = 0;
                            int bytesToRead = encryptedMessage.DecryptedSize;
                            using (MemoryStream ms = new MemoryStream())
                            {
                                while (bytesToRead > ms.Length)
                                {
                                    if (bytesToRead - ms.Length < buffer.Length)
                                        curRead = await csDecrypt.ReadAsync(buffer, 0, (int)(bytesToRead - ms.Length));
                                    else
                                        curRead = await csDecrypt.ReadAsync(buffer, 0, buffer.Length);

                                    ms.Write(buffer, 0, curRead); //write chunk to [wherever]
                                }
                                networkMessage = NetworkMessage.Deserialize(ms.ToArray());
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                #region logging
                //using (StreamWriter writer = new StreamWriter(File.Open(@"..\..\..\..\crypto_read_log.txt", FileMode.Create)))
                //{
                //    writer.WriteLine("read log");

                //    writer.WriteLine($"AES Key = size: {Aes.Key.Length}");
                //    for (int i = 0; i < Aes.Key.Length; i++)
                //        writer.Write(Aes.Key[i] + " ");
                //    writer.WriteLine("");

                //    writer.WriteLine($"AES IV = size: {encryptedMessage.IV.Length}");
                //    for (int i = 0; i < encryptedMessage.IV.Length; i++)
                //        writer.Write(encryptedMessage.IV[i] + " ");
                //    writer.WriteLine("");

                //    writer.WriteLine($"Encrypted = size {encryptedMessage.EncryptedNetworkMessage.Length}");
                //    for (int i = 0; i < encryptedMessage.EncryptedNetworkMessage.Length; i++)
                //        writer.Write(encryptedMessage.EncryptedNetworkMessage[i] + " ");
                //    writer.WriteLine("");

                //    writer.WriteLine($"Decrypted = size {encryptedMessage.DecryptedSize}");
                //}
                #endregion

            }
            return networkMessage;
        }

        private async Task<byte[]> InternalReadStreamAsync(int bytesToRead)
        {
            byte[] buffer = new byte[4096];
            int curRead = 0;
            using (MemoryStream ms = new MemoryStream())
            {
                while (bytesToRead > ms.Length)
                {
                    if (bytesToRead - ms.Length < buffer.Length)
                        curRead = await stream.ReadAsync(buffer, 0, (int)(bytesToRead - ms.Length));
                    else
                        curRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    ms.Write(buffer, 0, curRead); //write chunk to [wherever]
                }
                return ms.ToArray();
            }
        }

        public void Dispose()
        {
            Aes.Dispose();
        }
    }
}
