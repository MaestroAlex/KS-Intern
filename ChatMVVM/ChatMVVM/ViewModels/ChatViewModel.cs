using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatMVVM.ViewModels.Commands;
using ChatHandler;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Data;
using System.Collections.Generic;

namespace ChatMVVM.ViewModels
{
    class ChatViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ChatItemViewModel> _ChatNames { get; } = new ObservableCollection<ChatItemViewModel>();
        private Dictionary<string, string> _ChatHistories = new Dictionary<string, string>();
        public ICommand SendMsgPressed => new DelegateCommand(SendMessageButton, (obj) => !string.IsNullOrEmpty(_Message));
        public ICommand ChatRoomPressed => new DelegateCommand(ChatRoomButton);
        private Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        private string _CurrentChatRoom;
        private const string _GeneralChatRoom = "General";

        private ClientChatHandler _ChatHandler = ClientChatHandler.Instance();

        public event PropertyChangedEventHandler PropertyChanged;

        private string _History = "";

        private string _Message;

        public string UserName => _ChatHandler.UserName;

        public string Message
        {
            get
            {
                return _Message;
            }

            set
            {
                _Message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public string CurrentChatHistory
        {
            get
            {
                return _ChatHistories[_CurrentChatRoom];
            }
            set
            {
                _ChatHistories[_CurrentChatRoom] = value;
                OnPropertyChanged(nameof(CurrentChatHistory));
            }
        }

        public ChatViewModel()
        {
            CreateNewChatRoom(_GeneralChatRoom);
            Task.Run(StartChatListening);

        }

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public void ChatRoomButton(object obj)
        {
            _CurrentChatRoom = (obj as Button).Content.ToString();
        }

        public async void SendMessageButton(object obj)
        {
            if (Message.StartsWith("!new") && _CurrentChatRoom == _GeneralChatRoom)
            {
                Message = Message.Replace("!new", MsgKeys.NewChat);
            }
            else if (_CurrentChatRoom != _GeneralChatRoom)
            {
                Message = MsgKeys.ToUser + _CurrentChatRoom + "$" + Message;
            }
            else
            {
                CurrentChatHistory += DateTime.Now.ToShortTimeString() + "| " + _ChatHandler.UserName + " : " + Message + "\n";
            }

            await _ChatHandler.SendMessage(Message, "");
            Message = "";
        }

        private bool IsContainsMsgKeys(string message)
        {

            return false;
        }

        private void CreateNewChatRoom(string chatName)
        {
            string chatHistory = "";
            _ChatHistories.Add(chatName, chatHistory);
            _ChatNames.Add(new ChatItemViewModel(chatName));
            _CurrentChatRoom = chatName;
        }

        private void ParseReceivedMessage(string message)
        {
            if (message.Contains(MsgKeys.NewChat))
            {
                // message = message.Replace(MsgKeys.NewChat, "");
                message = message.Replace(" ", "");
                message = message.Substring(message.IndexOf("$") + MsgKeys.NewChat.Length);
                //message = message.Substring(message.IndexOf("n$"));

                _dispatcher.Invoke(new Action(() =>
               {
                   CreateNewChatRoom(message);
               }));

                return;
            }

            CurrentChatHistory += "->" + DateTime.Now.ToShortTimeString() + "| " + message + "\n";
        }

        private async Task StartChatListening()
        {
            while (true)
            {
                try
                {
                    if (_ChatHandler.socket.Connected)
                    {
                        var data = _ChatHandler.ReceiveMessage();
                        if (!string.IsNullOrEmpty(data))
                        {
                            ParseReceivedMessage(data);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
