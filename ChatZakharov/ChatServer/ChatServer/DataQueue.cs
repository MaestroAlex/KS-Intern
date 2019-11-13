using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitPackage;

namespace ChatServer
{
    class DataQueue
    {
        public Queue<Message> Messages { get; set; }
        public Queue<string> NewUsers { get; set; }
        public Queue<string> DeletedUsers { get; set; }
        public bool ConnectionCheckRequired { get; set; }
        public bool IsBusy
        {
            get
            {
                return Messages.Count != 0
                    || NewUsers.Count != 0
                    || DeletedUsers.Count != 0;
            }
        }

        public DataQueue()
        {
            Messages = new Queue<Message>();
            NewUsers = new Queue<string>();
            DeletedUsers = new Queue<string>();
            ConnectionCheckRequired = false;
        }
    }
}
