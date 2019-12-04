using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Net.Sockets;
using System.Net;
using QChat.Common.Net;
using QChat.Common;
using QChat.CLient.Messaging;

namespace QChat.CLient.Services
{
    class MessagingService : IHandler
    {
        private ContentRecieverTable _recievers;
        private ContentSenderTable _senders;

        private bool _continue;
        private SenderInfo _senderInfo;

        private Dispatcher _dispatcher;

        public event MessageRecievedEventHandler MessageRecieved;

        public MessagingService(ContentRecieverTable recievers, ContentSenderTable senders, SenderInfo sender)
        {
            _recievers = recievers;
            _senders = senders;
            _senderInfo = new SenderInfo
            {
                UserInfo = StaticProvider.GetInstanceOf<AuthorizationService>().AuthorizationInfo.UserInfo
            };

            _dispatcher = Dispatcher.CurrentDispatcher;
        }


        public void SendTextMessage(string text, RecieverInfo recieverInfo)
        {
            var messageBytes = Encoding.Unicode.GetBytes(text);
            var content = Content.Wrap(messageBytes);

            var messageHeader = new MessageHeader
            {
                ContentType = ContentType.Text,
                SenderInfo = _senderInfo,
                RecieverInfo = recieverInfo,
                Length = content.Length
            };

            var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connect();
            var message = new Common.Message(messageHeader, content);

            SendMessage(message, recieverInfo, connection, _senders.GetSender<TextSender>());
        }
        public async Task SendTextMessageAsync(string text, RecieverInfo recieverInfo)
        {
            var messageBytes = Encoding.UTF8.GetBytes(text);
            var content = Content.Wrap(messageBytes);

            var messageHeader = new MessageHeader
            {
                ContentType = ContentType.Text,
                SenderInfo = _senderInfo,
                RecieverInfo = recieverInfo,
                Length = content.Length
            };

            var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connection;
            var message = new Common.Message(messageHeader, content);

            await SendMessageAsync(message, recieverInfo, connection, _senders.GetSender<TextSender>());
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
            Task.WaitAll(
                connection.LockWriteAsync()
                );

            try
            {
                connection.Write(new RequestHeader(RequestIntention.Messaging).AsBytes(), 0, RequestHeader.ByteLength);
                connection.Write(message.Header.AsBytes(), 0, MessageHeader.ByteLength);
                sender.SendContent(connection, message.Content);
            }
            catch(Exception e)
            {
                return false;
            }
            finally
            {
                connection.ReleaseWrite();
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

        public void RecieveMessage<T>(T connection) where T : IConnection
        {
            var messageHeader = MessageHeader.FromConnection(connection);
            Message message;

            switch (messageHeader.ContentType)
            {
                case ContentType.Text:
                    var reciever = DefineReciever(messageHeader);
                    var content = reciever.GetContent(messageHeader, connection);
                    message = new Message(messageHeader, content);
                    break;
                default:
                    throw new ArgumentException();
            }

            _dispatcher.Invoke(() => MessageRecieved?.Invoke(this, new MessageRecievedEventArgs(message)));
        }
        public async Task RecieveMessageAsync<T>(T connection) where T : IConnection
        {
            var messageHeader = await MessageHeader.FromConnectionAsync(connection);
            Message message;

            switch (messageHeader.ContentType)
            {
                case ContentType.Text:
                    var reciever = DefineReciever(messageHeader);
                    var content = await reciever.GetContentAsync(messageHeader, connection);
                    message = new Message(messageHeader, content);
                    break;
                default:
                    throw new ArgumentException();
            }

            await Task.Run(() => MessageRecieved?.Invoke(this, new MessageRecievedEventArgs(message)));
        }

        public void Handle(IConnection connection)
        {
            RecieveMessage(connection);
        }
        public async Task HandleAsync(IConnection connection)
        {
            await RecieveMessageAsync(connection);
        }
    }    
}
