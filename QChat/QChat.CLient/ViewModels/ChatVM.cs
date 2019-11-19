using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using QChat.CLient.Messaging;


namespace QChat.CLient.ViewModels
{
    internal class ChatVM : DependencyObject
    {
        public List<Message> Messages
        {
            get;
            set;
        }


        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(int), typeof(ChatVM), new PropertyMetadata(String.Empty));



        public string TextDraft
        {
            get { return (string)GetValue(TextDraftProperty); }
            set { SetValue(TextDraftProperty, value); }
        }
        public static readonly DependencyProperty TextDraftProperty =
            DependencyProperty.Register("TextDraft", typeof(string), typeof(ChatVM), new PropertyMetadata(String.Empty));






        public ChatType ChatType { get; private set; }
        public ulong Id { get; private set; }       

    }

    internal enum ChatType
    {
        User = 0,
        Group,
        Room
    }
}
