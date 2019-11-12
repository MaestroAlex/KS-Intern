using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Server.Sessioning;
using QChat.Common;

namespace QChat.Server.Messaging
{
    class Group
    {
        public ulong Id { get; private set; }
        public Dictionary<ulong, UserInfo> _members;

        
    }
}
