using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitPackage
{
    [Serializable]
    public class Room
    {
        public string UserOwner { get; set; }
        public string Name { get; set; }
        public string HashPass { get; set; }
    }
}
