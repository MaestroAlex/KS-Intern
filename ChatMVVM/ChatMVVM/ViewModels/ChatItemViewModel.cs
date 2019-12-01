using System.Windows.Input;

namespace ChatMVVM.ViewModels
{
    public class ChatItemViewModel
    {
        private static int chatCounter = 0;
        public ChatItemViewModel(string clientName)
        {
            Name = clientName;
            chatCounter++;
        }
        public string Name { get; set; }
    }
}