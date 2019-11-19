using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.CLient.Messaging
{
    internal class Message
    {
        public MessageType Type { get; private set; }
        public object Content { get; private set; }


        public Message(MessageType type, object content)
        {
            Type = type;
            Content = content;
        }
    }    

    internal enum MessageType
    {
        Text = 0,
    }
}
