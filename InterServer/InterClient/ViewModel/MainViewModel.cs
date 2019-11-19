using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using InterClient.AppServices;

namespace InterClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly NetworkService networkService;
        private readonly NavigationServiceEx navigationService;
        private bool _isLogged = false;

        private string _currentChatName;

        public ObservableCollection<ChatHistoryViewModel> CurrentChatHistory { get; private set; }
        public ObservableCollection<ChatItemViewModel> ChatNames { get; } = new ObservableCollection<ChatItemViewModel>();

        private Dictionary<string, ObservableCollection<ChatHistoryViewModel>> _chatMap = new Dictionary<string, ObservableCollection<ChatHistoryViewModel>>();

        public MainViewModel(NetworkService networkService, NavigationServiceEx navigationService)
        {
            this.networkService = networkService;
            this.navigationService = navigationService;

            foreach(var room in this.networkService._chatKeys)
            {
                this.ChatNames.Add(new ChatItemViewModel() { Name = room.Key });
            }

            this._currentChatName = this.ChatNames.First().Name;

            this._chatMap.Add(this._currentChatName, new ObservableCollection<ChatHistoryViewModel>());

            this.CurrentChatHistory = this._chatMap[_currentChatName];

            this.networkService.MessageEvent += NetworkService_MessageEvent;

            Messenger.Default.Register<AppMessage>(this, this.OnChatUpdateMessage);
        }

        internal void CreateChatClicked()
        {

        }

        public void OnChatChanged(string v)
        {
            this._currentChatName = v;
            this.CurrentChatHistory = _chatMap[v];
            this.RaisePropertyChanged("CurrentChatHistory");
        }

        private void OnChatUpdateMessage(AppMessage obj)
        {
            this.ChatNames.Clear();
            foreach (var room in this.networkService._chatKeys)
            {
                this.ChatNames.Add(new ChatItemViewModel() { Name = room.Key });
                if(this._chatMap.ContainsKey(room.Key) == false)
                {
                    this._chatMap.Add(room.Key, new ObservableCollection<ChatHistoryViewModel>());
                }
            }
        }

        public async void SendButtonClick()
        {
            if(this.UserMessage.StartsWith("!create "))
            {
                var args = this.UserMessage.Replace(' ', '|').Substring("!create ".Length);
                await this.networkService.CreateChatMessage(args);
            }
            else if(this.UserMessage.StartsWith("!list"))
            {
                await this.networkService.GetUsersList();
            }
            else
            {
                await this.networkService.SendMessage(_currentChatName, this.UserMessage);
            }
            this.UserMessage = null;
        }

        internal void OnWindowsLoaded()
        {
            if(this.networkService.IsLogged == false)
            {
                this.navigationService.Navigate<MainViewModel>();
            }
        }

        private void NetworkService_MessageEvent(string message, string chatName)
        {
            this._chatMap[chatName].Add(new ChatHistoryViewModel() { Message = message.Substring(message.IndexOf('|') + 1)
                , User = message.Substring(0, message.IndexOf('|')) });
        }

        private string mainText;
        public string MainText
        {
            get => this.mainText;
            set => this.Set(ref this.mainText, value);
        }
        private string userMessage;
        public string UserMessage
        {
            get => this.userMessage;
            set => this.Set(ref this.userMessage, value);
        }

        private string createChatName;
        public string CreateChatName
        {
            get => this.createChatName;
            set => this.Set(ref this.createChatName, value);
        }
    }
}