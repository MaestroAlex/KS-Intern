using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            var encryptionTests = new EncryptionTests();

            encryptionTests.Test1();
            encryptionTests.Test2();
            encryptionTests.Test3();
        }
    }
}
