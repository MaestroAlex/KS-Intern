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
        private Chat _currentChat;
        public Chat CurrentChat
        {
            get => _currentChat;
            set
            {
                _currentChat = value;
                Update();
            }
        }



        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set
            {
                _currentChat.Name = value;
                SetValue(NameProperty, value);
            }
        }
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(ChatVM), new PropertyMetadata(string.Empty));



        public string TextDraft
        {
            get { return (string)GetValue(TextDraftProperty); }
            set
            {
                _currentChat.TextDraft = value;
                SetValue(TextDraftProperty, value);                
            }
        }
        public static readonly DependencyProperty TextDraftProperty =
            DependencyProperty.Register("TextDraft", typeof(string), typeof(ChatVM), new PropertyMetadata(string.Empty));

        public ObservableCollection<ClientMessage> Messages
        {
            get { return (ObservableCollection<ClientMessage>)GetValue(MessagesProperty); }
            set { SetValue(MessagesProperty, value); }
        }
        public static readonly DependencyProperty MessagesProperty =
            DependencyProperty.Register("Messages", typeof(ObservableCollection<ClientMessage>), typeof(ChatVM), new PropertyMetadata(null));


        public ChatVM()
        {
            CurrentChat = StaticProvider.GetInstanceOf<ChattingService>().GetDeafultChat(ChatType.RoomChat);
        }

        public void Update()
        {
            Name = _currentChat.Name;
            TextDraft = _currentChat.TextDraft;
            Messages = _currentChat.Messages;
        }

        public void SendTextMessage()
        {
            StaticProvider.GetInstanceOf<MessagingService>().
                SendTextMessage(TextDraft, new RecieverInfo { Id = _currentChat.Id, Type = GetRecieverType(_currentChat.Type)});
        }

        private RecieverType GetRecieverType(ChatType type)
        {
            switch (type)
            {
                case ChatType.GroupChat: return RecieverType.Group;
                case ChatType.RoomChat: return RecieverType.Room;
                case ChatType.UserChat: return RecieverType.User;
                default: throw new ArgumentException();
            }
        }
    }
}
