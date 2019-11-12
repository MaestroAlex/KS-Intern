using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Server.Messaging
{
    class GroupManager
    {
        private Dictionary<ulong, Group> _groups;


        public GroupManager()
        {
            _groups = new Dictionary<ulong, Group>();
        }

        public Group GetGroup(ulong id)
        {
            return _groups[id];
        }
    }
}
