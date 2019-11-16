using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Server.Sessioning;
using QChat.Common;

namespace QChat.Server.Messaging
{
    class Room
    {
        public ulong Id { get; private set; }
        public List<User> Members { get; private set; }

        public Room(ulong id)
        {
            Id = id;
            Members = new List<User>();
        }
        public Room(ulong id, IEnumerable<User> members)
        {
            Id = id;
            Members = new List<User>(members);
        }
    }
}
