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
        public Queue<Channel> NewChannels { get; set; }
        public Queue<Channel> DeletedChannels { get; set; }
        public bool ConnectionCheckRequired { get; set; }
        public bool IsBusy
        {
            get
            {
                return Messages.Count != 0
                    || NewChannels.Count != 0
                    || DeletedChannels.Count != 0;
            }
        }

        public DataQueue()
        {
            Messages = new Queue<Message>();
            NewChannels = new Queue<Channel>();
            DeletedChannels = new Queue<Channel>();
            ConnectionCheckRequired = false;
        }
    }
}
