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
    // TODO add async write read
    public class NetworkMessageStream : IDisposable
    {
        private AESKeys aesKeys;
        public AESKeys AESKeys
        {
            get => aesKeys;
            set
            {
                aesKeys = value;
                aes.Key = value.Key;
                aes.IV = value.IV;
            }
        }

        private NetworkStream stream;
        private BinaryWriter networkWriter;
        private BinaryReader networkReader;
        private Aes aes;

        public NetworkMessageStream(NetworkStream stream)
        {
            this.stream = stream;
            networkWriter = new BinaryWriter(stream);
            networkReader = new BinaryReader(stream);
            aes = Aes.Create();
            aesKeys = new AESKeys();
        }

        public void Write(NetworkMessage message)
        {
            byte[] byteArrayMessage = message.Serialize();
            byte[] resData = new byte[4 + byteArrayMessage.Length]; // 4 - obj size
            BitConverter.GetBytes(byteArrayMessage.Length).CopyTo(resData, 0);
            byteArrayMessage.CopyTo(resData, 4);
            networkWriter.Write(resData);
        }

        public NetworkMessage Read()
        {
            int dataSize = networkReader.ReadInt32();
            return NetworkMessage.Deserialize(networkReader.ReadBytes(dataSize));
        }

        public void WriteEncrypted(NetworkMessage message)
        {
            if (AESKeys.Key == null || AESKeys.Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (AESKeys.IV == null || AESKeys.IV.Length <= 0)
                throw new ArgumentNullException("IV");

            ICryptoTransform encryptor = aes.CreateEncryptor();
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (BinaryWriter bwEncrypt = new BinaryWriter(csEncrypt))
                    {
                        byte[] serializedMessage = message.Serialize();
                        int decryptedMessageSize = serializedMessage.Length;
                        bwEncrypt.Write(serializedMessage);

                        byte[] encryptedMessage = msEncrypt.ToArray();
                        byte[] resDataEncrypted = new byte[8 + encryptedMessage.Length]; // 4 - encrypted obj size (for NetworkStream to read) // 4 - decrypted obj size (for BinaryReader to read)

                        BitConverter.GetBytes(encryptedMessage.Length).CopyTo(resDataEncrypted, 0);
                        BitConverter.GetBytes(decryptedMessageSize).CopyTo(resDataEncrypted, 4);
                        encryptedMessage.CopyTo(resDataEncrypted, 8);
                        networkWriter.Write(resDataEncrypted);


                        //logging
                        using (StreamWriter writer = new StreamWriter(File.Open(@"..\..\..\..\crypto_write_log.txt", FileMode.Create)))
                        {
                            writer.WriteLine("write log");

                            writer.WriteLine($"AES Key = size: {aes.Key.Length}");
                            for (int i = 0; i < aes.Key.Length; i++)
                                writer.Write(aes.Key[i] + " ");
                            writer.WriteLine("");

                            writer.WriteLine($"AES IV = size: {aes.IV.Length}");
                            for (int i = 0; i < aes.IV.Length; i++)
                                writer.Write(aes.IV[i] + " ");
                            writer.WriteLine("");

                            writer.WriteLine($"Encrypted = size {encryptedMessage.Length}");
                            for (int i = 0; i < encryptedMessage.Length; i++)
                                writer.Write(encryptedMessage[i] + " ");
                            writer.WriteLine("");

                            writer.WriteLine($"Decrypted = size {serializedMessage.Length}");
                            for (int i = 0; i < serializedMessage.Length; i++)
                                writer.Write(serializedMessage[i] + " ");
                        }
                    }
                }
            }
        }

        public NetworkMessage ReadEncrypted()
        {
            if (AESKeys.Key == null || AESKeys.Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (AESKeys.IV == null || AESKeys.IV.Length <= 0)
                throw new ArgumentNullException("IV");

            int encryptedDataSize = networkReader.ReadInt32();
            int decryptedDataSize = networkReader.ReadInt32();
            byte[] EncryptedMessage = networkReader.ReadBytes(encryptedDataSize);

            NetworkMessage networkMessage = null;

            try
            {
                ICryptoTransform decryptor = aes.CreateDecryptor();
                using (MemoryStream msDecrypt = new MemoryStream(EncryptedMessage))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (BinaryReader brDecrypt = new BinaryReader(csDecrypt))
                        {
                            byte[] decr = brDecrypt.ReadBytes(decryptedDataSize);
                            networkMessage = NetworkMessage.Deserialize(decr);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);

                //logging
                using (StreamWriter writer = new StreamWriter(File.Open(@"..\..\..\..\crypto_read_log.txt", FileMode.Create)))
                {
                    writer.WriteLine("read log");

                    writer.WriteLine($"AES Key = size: {aes.Key.Length}");
                    for (int i = 0; i < aes.Key.Length; i++)
                        writer.Write(aes.Key[i] + " ");
                    writer.WriteLine("");

                    writer.WriteLine($"AES IV = size: {aes.IV.Length}");
                    for (int i = 0; i < aes.IV.Length; i++)
                        writer.Write(aes.IV[i] + " ");
                    writer.WriteLine("");

                    writer.WriteLine($"Encrypted = size {EncryptedMessage.Length}");
                    for (int i = 0; i < EncryptedMessage.Length; i++)
                        writer.Write(EncryptedMessage[i] + " ");
                }

            }
            return networkMessage;
        }

        public void Dispose()
        {
            aes.Dispose();
        }
    }

    [Serializable]
    public class NetworkMessage
    {
        public ActionEnum Action { get; set; }
        public object Obj { get; set; }

        public NetworkMessage(ActionEnum action, object obj = null)
        {
            Action = action;
            Obj = obj;
        }

        public byte[] Serialize()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream streamResult = new MemoryStream())
            {
                formatter.Serialize(streamResult, this);
                return streamResult.ToArray();
            }
        }

        public static NetworkMessage Deserialize(byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream streamResult = new MemoryStream())
            {
                streamResult.Write(data, 0, data.Length);
                streamResult.Seek(0, SeekOrigin.Begin);
                return (NetworkMessage)formatter.Deserialize(streamResult);
            }
        }
    }
}
