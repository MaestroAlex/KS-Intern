using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common;
using QChat.Common.Net;

namespace QChat.Server.Messaging
{
    interface IMessenger<T>
    {
        MessagingResult Handle(IConnection connection, MessageHeader header);
        Task<MessagingResult> HandleAsync(IConnection connection, MessageHeader header);
    }
}
