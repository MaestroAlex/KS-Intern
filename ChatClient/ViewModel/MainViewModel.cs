using GalaSoft.MvvmLight;
using ChatClient.Services;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ClientServerLib.Common;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using ClientServerLib.Additional;

namespace ChatClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly NetworkService netServices;
        public ObservableCollection<ChatRoom> Rooms { get; } = new ObservableCollection<ChatRoom>();
        private ObservableCollection<MessageItem> currentChatMessageHistory;
        public ObservableCollection<MessageItem> MessageHistory { get { return currentChatMessageHistory; } set { Set(ref currentChatMessageHistory, value); } } //= new ObservableCollection<MessageItem>();
        private Dictionary<string, ObservableCollection<MessageItem>> MessagesInChats = new Dictionary<string, ObservableCollection<MessageItem>>();

        public MainViewModel(NetworkService netServices)
        {
            this.netServices = netServices;
            netServices.onMessageReceived += ReceiveMessage;
            netServices.onNewRoomCreated += NewRoomCreated;
            netServices.onChangedRoom += ActiveRoomChanged;
        }

        private void NewRoomCreated(string roomName)
        {
            Rooms.Add(new ChatRoom(roomName));
            MessagesInChats.Add(roomName, new ObservableCollection<MessageItem>());
            if (MessageHistory == null)
                ActiveRoomChanged(roomName);
        }

        string message;
        public string Message
        {
            get { return message; }
            set { Set(ref message, value); }
        }

        private void ReceiveMessage(string userName, string message, string roomName)
        {
            MessagesInChats[roomName].Add(new MessageItem(userName, message));
        }

        public void SendMessage()
        {
            netServices.SendMessage(Message);
            Message = "";
        }

        public void GetHelpMessage()
        {
            netServices.SendMessage(ChatSyntax.HelpCmd);
        }

        public void ChatRoomSelected(object button)
        {
            netServices.ChatRoomSelected(((Button)button).Content.ToString());
        }

        private void ActiveRoomChanged(string roomName)
        {
            MessageHistory = MessagesInChats[roomName];
        }
    }
}