using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Server.Messaging
{
    class GroupManager
    {
        private Dictionary<int, Group> _groups;


        public GroupManager()
        {
            _groups = new Dictionary<int, Group>();
        }

        public Group GetGroup(int id)
        {
            return _groups[id];
        }
    }
}
