using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using QChat.Common;
using QChat.Common.Net;
using QChat.CLient.ViewModels;
using QChat.CLient.Messaging;

namespace QChat.CLient.Services
{
    class MessageHistoryService
    {
        private Dictionary<string, Dictionary<int, ObservableCollection<MessageVM>>> _histories;

        private ContentRecieverTable _recievers;

        public MessageHistoryService()
        {
            _recievers = new ContentRecieverTable();
            _histories = new Dictionary<string, Dictionary<int, ObservableCollection<MessageVM>>>();

            //TODO: Group and Private chat histories

            _histories.Add("room", new Dictionary<int, ObservableCollection<MessageVM>>());

            StaticProvider.GetInstanceOf<MessagingService>().MessageRecieved += OnMessageRecieved;
        }

        public ObservableCollection<MessageVM> GetHistory(string chatType, int id)
        {
            if (!_histories.ContainsKey(chatType)) throw new ArgumentException("Incorrect chat type parsed");

            if (!_histories[chatType].TryGetValue(id, out var messages))
            {
                if (chatType.Equals("room"))
                {
                    messages = new ObservableCollection<MessageVM>();
                }
                else
                {
                    messages = GetHistoryFomServer(chatType, id);                    
                }

                _histories[chatType].Add(id, messages);
            }

            return messages;
        }

        private ObservableCollection<MessageVM> GetHistoryFomServer(string chatType, int id)
        {
            var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connect();

            var requestHeaderBytes = new RequestHeader(RequestIntention.HistorySynchronization).AsBytes();

            RecieverType recieverType;
            switch (chatType)
            {
                case "room":
                    recieverType = RecieverType.Room;
                    break;
                case "user":
                    recieverType = RecieverType.User;
                    break;
                case "group":
                    recieverType = RecieverType.Group;
                    break;
                default:
                    recieverType = RecieverType.None;
                    break;
            }


            var synchronizationHeaderBytes = new HistorySynchronizationHeader(recieverType, id).AsBytes();

            Task.WaitAll(
                connection.LockReadAsync(),
                connection.LockWriteAsync()
                );

            var result = new ObservableCollection<MessageVM>();

            var flagBytes = new byte[sizeof(bool)];

            bool GetMessageAvailableFlag()
            {
                if (connection.Read(flagBytes, 0, sizeof(bool)) <= 0) return false;
                return BitConverter.ToBoolean(flagBytes, 0);
            };

            var socialService = StaticProvider.GetInstanceOf<SocialService>();

            try
            {
                connection.Write(requestHeaderBytes, 0, RequestHeader.ByteLength);
                connection.Write(synchronizationHeaderBytes, 0, HistorySynchronizationHeader.ByteLength);                

                while (GetMessageAvailableFlag())
                {
                    var messageHeader = MessageHeader.FromConnection(connection);
                    var reciever = _recievers.DefineReciever(messageHeader.ContentType);

                    if (reciever == null) return new ObservableCollection<MessageVM>();
                    var content = reciever.GetContent(messageHeader, connection);

                    var senderName = socialService.GetName(messageHeader.SenderInfo.UserInfo.Id);

                    result.Add(new MessageVM(senderName, messageHeader.ContentType.ToString(), _recievers.ToObject(messageHeader.ContentType, content)));
                }

                return result;
            }
            finally
            {
                connection.ReleaseRead();
                connection.ReleaseWrite();
            }
        }

        private void OnMessageRecieved(object sender, MessageRecievedEventArgs args)
        {
            var message = args.Message;
            var header = message.Header;
            var chatType = string.Empty;

            switch (header.RecieverInfo.Type)
            {
                case RecieverType.Room:
                    chatType = "room";
                    break;
                default:
                    return;
            }

            if (!_histories["room"].TryGetValue(header.RecieverInfo.Id, out var messages)) return;

            var senderName = StaticProvider.GetInstanceOf<SocialService>().GetName(header.SenderInfo.UserInfo.Id);

            messages.Add(new MessageVM(senderName, "text", Encoding.Unicode.GetString(message.Content.AsBytes())));
        }
    }
}
