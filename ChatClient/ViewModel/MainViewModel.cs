using GalaSoft.MvvmLight;
using ChatClient.Services;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ClientServerLib;
using System.Windows.Controls;

namespace ChatClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly NetworkService netServices;
        public ObservableCollection<Room> Rooms { get; } = new ObservableCollection<Room>();

        public MainViewModel(NetworkService netServices)
        {
            this.netServices = netServices;
            netServices.onMessageReceived += AddMessageToChatHistory;
            netServices.onNewRoomCreated += NewRoomCreated;
        }

        private void NewRoomCreated(string roomName)
        {
            Rooms.Add(new Room(roomName));
        }

        string message;
        public string Message
        {
            get { return message; }
            set { Set(ref message, value); }
        }
        string chathistory = "»стори€ сообщений будет отображатьс€ здесь";
        public string ChatHistory
        {
            get { return chathistory; }
            set { Set(ref chathistory, value); }
        }


        private void AddMessageToChatHistory(string message)
        {
            ChatHistory += "\n" + message;
        }

        public void SendMessage()
        {
            netServices.SendMessage(Message);
            Message = "";
        }

        public void GetHelpMessage()
        {
            netServices.SendMessage("/help");
        }

        public void ChatRoomSelected(object button)
        {
            netServices.ChatRoomSelected(((Button)button).Content.ToString());
        }
    }
}