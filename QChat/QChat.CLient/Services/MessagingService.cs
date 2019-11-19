using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using QChat.Common.Net;
using QChat.Common;
using QChat.CLient.Messaging;

namespace QChat.CLient.Services
{
    class MessagingService
    {
        private ContentRecieverTable _recievers;
        private ContentSenderTable _senders;

        private SenderInfo _senderInfo;

        public event MessageRecievedEventHandler MessageRecieved;

        public MessagingService(ContentRecieverTable recievers, ContentSenderTable senders, SenderInfo sender)
        {
            _recievers = recievers;
            _senders = senders;
        }


        public bool SendTextMessage(string message, RecieverInfo recieverInfo, IConnection connection)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var content = Content.Wrap(messageBytes);

            var messageHeader = new MessageHeader
            {
                ContentType = ContentType.Text,
                SenderInfo = _senderInfo,
                RecieverInfo = recieverInfo,
                Length = content.Length
            };

            return SendMessage(new Common.Message(messageHeader, content), recieverInfo, connection, _senders.GetSender<TextSender>());
        }
        public async Task<bool> SendTextMessageAsync(string message, RecieverInfo recieverInfo, IConnection connection)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var content = Content.Wrap(messageBytes);

            var messageHeader = new MessageHeader
            {
                ContentType = ContentType.Text,
                SenderInfo = _senderInfo,
                RecieverInfo = recieverInfo,
                Length = content.Length
            };

            return await SendMessageAsync(new Common.Message(messageHeader, content), recieverInfo, connection, _senders.GetSender<TextSender>());
        }

        private IContentReciever DefineReciever(MessageHeader header)
        {
            switch (header.ContentType)
            {
                case ContentType.Text:
                    return _recievers.Get<TextReciever>();
                default:
                    throw new NotImplementedException("Needed reciever wasn't available");
            }
        }

        private bool SendMessage(Common.Message message, RecieverInfo recieverInfo, IConnection connection, IContentSender sender)
        {
            try
            {
                connection.Write(new RequestHeader(RequestIntention.Messaging).AsBytes(), 0, RequestHeader.ByteLength);
                connection.Write(message.Header.AsBytes(), 0, MessageHeader.ByteLength);
                sender.SendContent(connection, message.Content);
            }
            catch
            {
                return false;
            }

            return true;
        }
        private async Task<bool> SendMessageAsync(Common.Message message, RecieverInfo recieverInfo, IConnection connection, IContentSender sender)
        {
            try
            {
                await connection.WriteAsync(new RequestHeader(RequestIntention.Messaging).AsBytes(), 0, RequestHeader.ByteLength);
                await connection.WriteAsync(message.Header.AsBytes(), 0, MessageHeader.ByteLength);
                await sender.SendContentAsync(connection, message.Content);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }    
}
