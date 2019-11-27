using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.CLient.Messaging
{
    class ClientMessage
    {
        public string Sender { get; private set; } 
        public Type ContentType { get; private set; }
        public object Content { get; private set; }

        public ClientMessage(string sender, object content)
        {
            Sender = sender;
            ContentType = content.GetType();
            Content = content;
        }
    }

}
