using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModel
{
    class ChatRoomItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ChatRoomItem(int Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
        }
    }
}
