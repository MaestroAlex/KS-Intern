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
        public ObservableCollection<Chat> _Chats { get; } = new ObservableCollection<Chat>();

        public ICommand SendMsgPressed => new DelegateCommand(SendMessageButton, (obj) => !string.IsNullOrEmpty(_Message));
        public ICommand ChatRoomPressed => new DelegateCommand(ChatRoomButton);


        private Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        private int _CurrentChatID = 1;
        private const int _GeneralChatRoomID = 1;

        private ClientChatHandler _ChatHandler = ClientChatHandler.Instance();

        public event PropertyChangedEventHandler PropertyChanged;

        private string _Message;

        public string UserName => _ChatHandler.Username;

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
            get
            {
                string result = "";
                foreach (var chat in _Chats)
                {
                    if (chat.ID == CurrentChatID)
                    {
                        result = chat.History;
                    }
                }
                return result;
            }
            set
            {
                foreach (var chat in _Chats)
                {
                    if (chat.ID == CurrentChatID)
                    {
                        chat.History = value;
                    }
                }
                OnPropertyChanged(nameof(CurrentChatHistory));
            }
        }

        public int CurrentChatID
        {
            get => _CurrentChatID;
            set
            {
                _CurrentChatID = value;
                OnPropertyChanged(nameof(CurrentChatHistory));
            }
        }

        public ChatViewModel()
        {
            CreateNewChatRoom(_GeneralChatRoomID, "General");
            _ChatHandler.SendMessage(MsgKeys.ChatHistory + MsgKeys.End);
            Task.Factory.StartNew(StartChatListening);
        }

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public void ChatRoomButton(object obj)
        {
            var chatName = (obj as Button).Content.ToString();
            if (chatName == "General")
                CurrentChatID = 1;

            foreach (var chat in _Chats)
            {
                if (chat.Name == chatName)
                {
                    CurrentChatID = chat.ID;
                }
            }
        }

        public async void SendMessageButton(object obj)
        {
            if (Message.StartsWith("!new") && _CurrentChatID == _GeneralChatRoomID)
            {
                Message = Message.Replace(" ", "");
                Message = Message.Substring("!new".Length);
                Message = $"{MsgKeys.NewChat}|{Message}";
            }

            else if (_CurrentChatID == _GeneralChatRoomID)
            {
                Message = $"{MsgKeys.GeneralChat}|{UserName}|{Message}";
            }

            else if (_CurrentChatID != _GeneralChatRoomID)
            {
                int ID = 1;
                string chatName = "";
                foreach (var chat in _Chats)
                {
                    if (chat.ID == _CurrentChatID)
                    {
                        ID = chat.ID;
                        chatName = chat.Name;
                        break;
                    }
                }

                Message = $"{MsgKeys.ToChat}|{UserName}|{ID}|{chatName}|{Message}";
            }
            else
            {
                CurrentChatHistory += DateTime.Now.ToShortTimeString() + "| " + _ChatHandler.Username + " : " + Message + "\n";
            }

            _ChatHandler.SendMessage(Message, "");
            Message = "";
        }

        private bool IsContainsMsgKeys(string message)
        {
            return false;
        }

        private void CreateNewChatRoom(int ID, string chatName)
        {
            _dispatcher?.Invoke(new Action(() =>
            {
                foreach (var chat in _Chats)
                {
                    if (chat.ID == ID || chat.Name == chatName)
                        return;
                }
                _Chats.Add(new Chat(ID, chatName, $"{UserName} to {chatName}\n"));
            }));
        }

        private bool Histories(string message)
        {
            bool result = false;
            int historiesCounter = 0;
            var data = message.Split('*');
            foreach (var a in data)
            {
                if (a.Contains(MsgKeys.ChatHistory))
                    historiesCounter++;
            }

            result = historiesCounter > 0 ? true : false;

            return result;
        }

        private void ParseReceivedMessage(string message)
        {
            message = message.Substring(0, message.LastIndexOf(MsgKeys.End));


            if (message.StartsWith(MsgKeys.ServerAnswer))
            {
                message = message.Substring((MsgKeys.ServerAnswer + "|").Length);
                WriteToChatHistory(message, _GeneralChatRoomID);

            }

            else if (message.StartsWith(MsgKeys.NewChat))
            {
                var messageData = message.Split('|');

                CreateNewChatRoom(int.Parse(messageData[1]), messageData[2]);

            }

            else if (message.StartsWith(MsgKeys.GeneralChat))
            {
                string[] data = message.Split('|');
                message = $"{data[1]}:{data[2]}";
                WriteToChatHistory(message, _GeneralChatRoomID);
            }

            else if (message.StartsWith(MsgKeys.ToChat))
            {
                bool chatExists = false;
                var messageData = message.Split('|');
                var sender = messageData[1];
                var chatID = int.Parse(messageData[2]);
                var chatName = messageData[3];
                var msg = $"{sender}:{messageData[4]}";

                foreach (var chat in _Chats)
                {
                    if (chat.ID == chatID)
                    {
                        WriteToChatHistory(msg, chat.ID);
                        chatExists = true;
                    }
                }
                if (!chatExists)
                {
                    CreateNewChatRoom(chatID, chatName);
                    WriteToChatHistory(msg, chatID);
                }
            }

            else if (message.StartsWith(MsgKeys.JoinedRoom))
            {
                message = message.Substring((MsgKeys.JoinedRoom + "|").Length);
                message = $"User {message} joined the room.";
                WriteToChatHistory(message, _GeneralChatRoomID);
            }

            /* else if (message.StartsWith(MsgKeys.ChatHistory))
             {


                  message = message.Substring(5);
                  var messageData = message.Split('~');

                  for (int i = 0; i < messageData.Length; i++)
                  {
                      var msg = Encrypt.DecodeMessage(messageData[i]);
                      ParseReceivedMessage(msg).Wait();
                  }
             }*/
        }

        private void ParseHistory(string history)
        {
            history = history.Substring(5);
            var messageData = history.Split('~');
            Console.WriteLine(history);
            foreach (var a in messageData)
            {
                if (!string.IsNullOrEmpty(a))
                {
                    var msg = Encrypt.DecodeMessage(a);
                    ParseReceivedMessage(msg);
                }
            } 
        }

        private void WriteToChatHistory(string message, int chatID, bool timeMark = false)
        {
            foreach (var chat in _Chats)
            {
                if (chat.ID == chatID)
                {
                    chat.History += (timeMark ? (DateTime.Now.ToShortTimeString() + "| ") : ("")) + message + "\n";
                    OnPropertyChanged(nameof(CurrentChatHistory));
                }
            }
        }

        private async Task StartChatListening()
        {
            while (true)
            {
                try
                {
                    if (_ChatHandler.client.Connected)
                    {
                        var data = _ChatHandler.ReceiveMessage();
                        //data = await Encrypt.DecodeMessage(data);
                        if (!string.IsNullOrEmpty(data))
                        {
                            if (Histories(data))
                            {
                                var histories = data.Split('*');
                                foreach (var a in histories)
                                {
                                    if (!string.IsNullOrEmpty(a))
                                        ParseHistory(a);
                                }
                            }
                            else
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
