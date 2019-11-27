using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using QChat.CLient.Messaging;

namespace QChat.CLient.ViewModels
{
    class MainVM : DependencyObject
    {
        public ChatSelectionItem SelectedChat
        {
            get { return (ChatSelectionItem)GetValue(SelectedChatProperty); }
            set
            {                
                SetValue(SelectedChatProperty, value);
            }
        }
        public static readonly DependencyProperty SelectedChatProperty =
            DependencyProperty.Register("SelectedChat", typeof(ChatSelectionItem), typeof(MainVM), new PropertyMetadata(null));



        public ObservableCollection<ChatSelectionItem> Chats
        {
            get { return (ObservableCollection<ChatSelectionItem>)GetValue(ChatsProperty); }
            set { SetValue(ChatsProperty, value); }
        }
        public static readonly DependencyProperty ChatsProperty =
            DependencyProperty.Register("Chats", typeof(ObservableCollection<ChatSelectionItem>), typeof(MainVM), new PropertyMetadata(null));
    }
}
