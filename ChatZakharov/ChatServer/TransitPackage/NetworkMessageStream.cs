using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace TransitPackage
{
    // TODO add async write read
    public class NetworkMessageStream
    {
        private NetworkStream stream;
        private BinaryWriter writer;
        private BinaryReader reader;
        public NetworkMessageStream(NetworkStream stream)
        {
            this.stream = stream;
            writer = new BinaryWriter(stream);
            reader = new BinaryReader(stream);
        }
        public void Write(NetworkMessage message)
        {
            byte[] byteArrayMessage = message.Serialize();
            byte[] resData = new byte[4 + byteArrayMessage.Length];// 4 - obj size
            BitConverter.GetBytes(byteArrayMessage.Length).CopyTo(resData, 0);
            byteArrayMessage.CopyTo(resData, 4);
            writer.Write(resData);
        }

        public NetworkMessage Read()
        {
            int dataSize = reader.ReadInt32();
            return NetworkMessage.Deserialize(reader.ReadBytes(dataSize));
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
