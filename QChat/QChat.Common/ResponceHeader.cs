using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Common
{
    public struct ResponceHeader
    {
        public int Version;
        public ResponceIntention Intent;
    }

    public enum ResponceIntention
    {
        Authorization = 0,
        Messaging,
        Registration,
        Disconecting,
        Rooming,
    }
}
