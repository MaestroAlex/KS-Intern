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


       // public ObservableCollection<Chat> _Chats { get; } = new ObservableCollection<Chat>();

            

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
            get => _Message;

            set
            {
                _Message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public string CurrentChatHistory
        {
            get => _ChatHistories[_CurrentChatRoom];
            set
            {
                _ChatHistories[_CurrentChatRoom] = value;
                OnPropertyChanged(nameof(CurrentChatHistory));
            }
        }

        public string CurrentChatRoom
        {
            get => _CurrentChatRoom;
            set
            {
                _CurrentChatRoom = value;
                OnPropertyChanged(nameof(CurrentChatHistory));
            }
        }

        public ChatViewModel()
        {
            CreateNewChatRoom(_GeneralChatRoom,0);
            Task.Run(StartChatListening);
        }

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public void ChatRoomButton(object obj)
        {
            CurrentChatRoom = (obj as Button).Content.ToString();
        }

        public async void SendMessageButton(object obj)
        {
            if (Message.StartsWith("!new") && _CurrentChatRoom == _GeneralChatRoom)
            {
                Message = Message.Replace(" ", "");
                Message = Message.Substring("!new".Length);
                Message = $"{MsgKeys.NewChat}|{Message}";
            }

            else if (_CurrentChatRoom == _GeneralChatRoom)
            {
                Message = $"{MsgKeys.GeneralChat}|{UserName}|{Message}";
            }

            else if (_CurrentChatRoom != _GeneralChatRoom)
            {
                int ID = 0;
                foreach(var chat in _ChatNames)
                {
                    if(chat.Name==_CurrentChatRoom)
                    {
                        ID = chat.ID;
                        break;
                    }
                }

                Message = $"{MsgKeys.ToChat}|{UserName}|{ID}|{Message}";
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

        private void CreateNewChatRoom(string chatName,int ID)
        {
            _ChatHistories.Add(chatName, "");
            _ChatHistories[chatName] += $"{UserName} to {chatName}\n";
            _ChatNames.Add(new ChatItemViewModel(chatName,ID));

            CurrentChatRoom = chatName;
        }

        private void ParseReceivedMessage(string message)
        {
            if (message.StartsWith(MsgKeys.ServerAnswer))
            {
                message = message.Substring((MsgKeys.ServerAnswer + "|").Length);
                WriteToChatHistory(message, _GeneralChatRoom, false);

            }

            else if (message.StartsWith(MsgKeys.NewChat))
            {
                var messageData = message.Split('|');

                
                _dispatcher.Invoke(new Action(() =>
               {
                   CreateNewChatRoom(messageData[2], int.Parse(messageData[1]));
               }));
            }

            else if (message.StartsWith(MsgKeys.GeneralChat))
            {
                message = message.Substring((MsgKeys.GeneralChat + "|").Length);
                string[] data = message.Split('|');
                message = $"{data[0]}:{data[1]}";
                WriteToChatHistory(message, _GeneralChatRoom);

            }

            else if(message.StartsWith(MsgKeys.ToChat))
            {
                var messageData = message.Split('|');

                var sender = messageData[1];
                var chatID = int.Parse(messageData[2]);
                var msg = messageData[3];
                msg = $"{sender}:{msg}";

                foreach(var chat in _ChatNames)
                {
                    if(chat.ID==chatID)
                    {
                        WriteToChatHistory(msg, chat.Name);
                        //_ChatHistories[chat.Name] += msg;
                    }
                }
            }

            else if (message.StartsWith(MsgKeys.JoinedRoom))
            {
                message = message.Substring((MsgKeys.JoinedRoom + "|").Length);
                message = $"User {message} joined the room.";
                WriteToChatHistory(message, _GeneralChatRoom);
            }

            else if (message.Contains(UserName + "$"))
            {
                message = message.Substring(message.IndexOf(UserName + "$"));
            }
        }

        private void WriteToChatHistory(string message, string chatHistory, bool timeMark = true)
        {
            if (_ChatHistories.ContainsKey(chatHistory))
            {
                //CurrentChatRoom =
                _ChatHistories[chatHistory] += (timeMark ? (DateTime.Now.ToShortTimeString() + "| ") : ("")) + message + "\n";
                OnPropertyChanged(nameof(CurrentChatHistory));
            }
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
