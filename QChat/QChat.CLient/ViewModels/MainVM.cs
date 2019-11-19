using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using QChat.CLient.Messaging;

namespace QChat.CLient.ViewModels
{
    class MainVM : DependencyObject
    {
        private MessageRecievedEventHandler _messageRecievedEventHandler;       


        public ChatVM CurrentChat
        {
            get { return (ChatVM)GetValue(CurrentChatProperty); }
            set { SetValue(CurrentChatProperty, value); }
        }

        public static readonly DependencyProperty CurrentChatProperty =
            DependencyProperty.Register("CurrentChat", typeof(ChatVM), typeof(MainVM), new PropertyMetadata(null));     
        

        public MainVM()
        {
            _messageRecievedEventHandler += OnMessageRecieved;
        }
        
        private void OnMessageRecieved(object sender, MessageRecievedEventArgs eventArgs)
        {
            var message = eventArgs.Message;
            

        }
    }
}
