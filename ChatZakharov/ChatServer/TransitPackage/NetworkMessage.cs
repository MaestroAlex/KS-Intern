using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TransitPackage
{
    [Serializable]
    public class NetworkMessage : SerializationBase<NetworkMessage>
    {
        public ActionEnum Action { get; set; }
        public object Obj { get; set; }

        public NetworkMessage(ActionEnum action, object obj = null)
        {
            Action = action;
            Obj = obj;
        }
    }
}
