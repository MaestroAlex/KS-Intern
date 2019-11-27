using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitPackage
{
    public enum ChannelType { public_open, public_closed, user}

    [Serializable]
    public class Channel
    {
        public string Name { get; set; }
        public bool IsEntered { get; set; }
        public ChannelType Type { get; set; }
    }
}
