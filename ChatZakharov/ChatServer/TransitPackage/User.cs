using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitPackage
{
    [Serializable]
    public class User
    {
        public string Login { get; set; }
        public string HashPass { get; set; }
    }
}
