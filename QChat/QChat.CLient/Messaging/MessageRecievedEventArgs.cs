using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.CLient.Messaging
{
    class MessageRecievedEventArgs : EventArgs
    {
        public Common.Message Message;

        public MessageRecievedEventArgs(Common.Message message)
        {
            Message = message;
        }
    }
}
