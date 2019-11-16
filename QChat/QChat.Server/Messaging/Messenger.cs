using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;
using QChat.Common;

namespace QChat.Server.Messaging
{
    abstract class Messenger<T> : IMessenger<T>
    {
        protected ContentSenderTable _senders;
        protected ContentRecieverTable _recievers;


        protected Messenger(ContentSenderTable senders, ContentRecieverTable recievers)
        {
            _senders = senders;
            _recievers = recievers;
        }


        protected IContentSender DefineSender(Message message)
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
        protected IContentReciever DefineReciever(MessageHeader header)
        {
            switch (header.ContentType)
            {
                case ContentType.Text:
                    return _recievers.Get<TextReciever>();
                default:
                    throw new NotImplementedException("Needed reciever wasn't available");
            }
        }

        protected void SendMessageHeader(MessageHeader header, Connection connection)
        {
            var bytes = header.AsBytes();
            connection.Write(bytes, 0, MessageHeader.ByteLength);
        }
        protected async Task SendMessageHeaderAsync(MessageHeader header, Connection connection)
        {
            var bytes = header.AsBytes();
            await connection.WriteAsync(bytes, 0, MessageHeader.ByteLength);
        }

        public abstract MessagingResult Handle(IConnection connection, MessageHeader header);
        public abstract Task<MessagingResult> HandleAsync(IConnection connection, MessageHeader header);

        protected abstract Message RecieveMessage(IConnection connection, MessageHeader header);
        protected abstract Task<Message> RecieveMessageAsync(IConnection connection, MessageHeader header);

        protected abstract void SendMessage(T reciever, Message message);
        protected abstract Task SendMessageAsync(T reciever, Message message);
    }
}
