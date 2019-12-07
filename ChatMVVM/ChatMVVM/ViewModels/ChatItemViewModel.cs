using System.Windows.Input;

namespace ChatMVVM.ViewModels
{
    public class ChatItemViewModel
    {
        private static int chatCounter = 1;
        private int _ID;

        public int ID => _ID;
        public ChatItemViewModel(string clientName,int chatID)
        {
            _ID = chatID;
            Name = clientName;
            chatCounter++;
        }
        public string Name { get; set; }
    }
}