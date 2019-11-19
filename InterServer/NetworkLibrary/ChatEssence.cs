using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLibrary
{
    public class ChatEssence
    {
        private List<ClientEssence> _chatClients = new List<ClientEssence>();
        private string _chatName = "";
        public ChatEssence(string chatName)
        {
            this._chatName = chatName;
        }

        public List<ClientEssence> Users => this._chatClients;

        public string ChatName { get => _chatName; }

        public void AddClientToChat(ClientEssence client)
        {
            this._chatClients.Add(client);
        }

        public async void WriteToChat(string message)
        {
            foreach(var cl in _chatClients)
            {
                
            }
        }
    }
}
