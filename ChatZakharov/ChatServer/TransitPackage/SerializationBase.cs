using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace TransitPackage
{
    [Serializable]
    public class SerializationBase<T>
    {
        public byte[] Serialize()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream streamResult = new MemoryStream())
            {
                formatter.Serialize(streamResult, this);
                return streamResult.ToArray();
            }
        }

        public static T Deserialize(byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream streamResult = new MemoryStream())
            {
                streamResult.Write(data, 0, data.Length);
                streamResult.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(streamResult);
            }
        }
    }
}
