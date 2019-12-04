using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using QChat.CLient.Messaging;
using QChat.CLient.Services;
using QChat.Common;

namespace QChat.CLient.ViewModels
{
    class ChatVM : DependencyObject
    {
        private MessagingService _messagingService;
        private MessageHistoryService _historyService;

        public string MessageText
        {
            get { return (string)GetValue(MessageTextProperty); }
            set { SetValue(MessageTextProperty, value); }
        }
        public static readonly DependencyProperty MessageTextProperty =
            DependencyProperty.Register("MessageText", typeof(string), typeof(ChatVM), new PropertyMetadata(string.Empty));

        public ObservableCollection<MessageVM> MessageList
        {
            get { return (ObservableCollection<MessageVM>)GetValue(MessageListProperty); }
            set { SetValue(MessageListProperty, value); }
        }
        public static readonly DependencyProperty MessageListProperty =
            DependencyProperty.Register("MessageList", typeof(ObservableCollection<MessageVM>), typeof(ChatVM), new PropertyMetadata(null));

        public string Type { get; set; }

        private int _id = 0;
        public int Id
        {
            get => _id;
            set
            {
                if (value == _id) return;

                _id = value;
                MessageList = _historyService.GetHistory(Type, _id);
            }
        }

        public ChatVM()
        {
            _messagingService = StaticProvider.GetInstanceOf<MessagingService>();
            _historyService = StaticProvider.GetInstanceOf<MessageHistoryService>();
        }

        public async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageText)) return;
            var messageText = MessageText;
            MessageText = string.Empty;
            await Task.Run(() => _messagingService.SendTextMessage(
                messageText, 
                new RecieverInfo
                {
                    Type = GetRecieverType(Type),
                    Id = this.Id}
                )
            );
        }

        private RecieverType GetRecieverType(string type)
        {
            switch (type)
            {
                case "room":
                    return RecieverType.Room;
                case "group":
                    return RecieverType.Group;
                case "user":
                    return RecieverType.User;
                default:
                    return RecieverType.None;
            }
        }
    }
}
