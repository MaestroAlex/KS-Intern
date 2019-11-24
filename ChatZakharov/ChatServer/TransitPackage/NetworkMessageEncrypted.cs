using System;

namespace TransitPackage
{
    [Serializable]
    internal class NetworkMessageEncrypted : SerializationBase<NetworkMessageEncrypted>
    {
        internal byte[] EncryptedNetworkMessage { get; set; }
        internal byte[] IV { get; set; }
        internal int DecryptedSize { get; set; }

        internal NetworkMessageEncrypted(byte[] EncryptedNetworkMessage, byte[] IV, int DecryptedSize)
        {
            this.EncryptedNetworkMessage = EncryptedNetworkMessage;
            this.IV = IV;
            this.DecryptedSize = DecryptedSize;
        }
    }
}
