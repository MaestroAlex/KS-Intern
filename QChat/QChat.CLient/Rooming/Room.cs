using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common;

namespace QChat.CLient.Rooming
{
    class Room
    {
        public int Id { get; private set; }        
        public bool Connected { get; private set; }
        public bool IsPublic { get; private set; }
        public string Name { get; private set; }

        public Room(RoomInfo info, string name)
        {
            Id = info.Id;
            Name = name;
        }
    }
}
