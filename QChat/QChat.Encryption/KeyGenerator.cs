using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Encryption
{
    public static class KeyGenerator
    {
        private static Random _random = new Random();

        public static int Generate(KeyVariant variant, out byte[] key)
        {
            switch (variant)
            {
                case KeyVariant.Aes128:
                    key = new byte[128 / 8];
                    _random.NextBytes(key);
                    return 128;
                default:
                    key = new byte[0];
                    return 0;
            }
        }
    }
}
