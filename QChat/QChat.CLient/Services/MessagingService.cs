using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using QChat.Common.Net;
using QChat.Common;

namespace QChat.CLient.Services
{
    class MessagingService
    {
        private ContentRecieverTable _recievers;
        private ContentSenderTable _senders;


        public MessagingService(ContentRecieverTable recievers, ContentSenderTable senders)
        {

        }


        public IContentSender DefineSender(Message message)
        {
            IContentSender sender;

            switch (message.Header.ContentType)
            {
                case ContentType.Text:
                    sender = _senders.GetSender<TextSender>();
                    break;
                default:
                    throw new NotImplementedException();
            }

            return sender;
        }
        public IContentReciever DefineReciever(MessageHeader header)
        {
            switch (header.ContentType)
            {
                case ContentType.Text:
                    return _recievers.Get<TextReciever>();
                default:
                    throw new NotImplementedException("Needed reciever wasn't available");
            }
        }
    }
}
