using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace QChat.CLient.Messaging
{
    class Chat
    {
        public ObservableCollection<ClientMessage> Messages { get; private set; }
        
        public string TextDraft { get; set; }
        public string Name { get; set; }
        public ChatType Type { get; private set; }
        public int Id { get; private set; }

        public Chat(string name, string textDraft, IEnumerable<ClientMessage> messages, ChatType type)
        {
            Type = type;
            Messages = new ObservableCollection<ClientMessage>(messages);
            Name = name;
            TextDraft = textDraft;
            Id = name.GetHashCode();
        }
    }

    enum ChatType
    {
        UserChat = 0,
        GroupChat,
        RoomChat
    }
}
