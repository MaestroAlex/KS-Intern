using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common;

namespace QChat.CLient.Messaging
{
    class MessageRecievedEventArgs : EventArgs
    {
        public Common.Message Message;

        public MessageRecievedEventArgs(Message message)
        {
            Message = message;
        }
    }
}
