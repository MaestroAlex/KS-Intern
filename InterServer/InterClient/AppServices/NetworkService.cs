using GalaSoft.MvvmLight.Messaging;
using InterClient.ViewModel;
using NetworkLibrary;
using NetworkLibrary.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InterClient.AppServices
{
    public class NetworkService
    {
        public delegate void MessageRecieved(string message, string chatName);
        public event MessageRecieved MessageEvent;

        private string UserLogin;

        private MainClient _client;
        private bool _isLogged;

        public Dictionary<string, int> _chatKeys = new Dictionary<string, int>();

        public NetworkService()
        {
            _client = new MainClient();
            _client.MessageEvent += _client_MessageEvent;
            StartClient();
        }

        public bool IsLogged { get => _isLogged; private set => _isLogged = value; }

        private async Task<bool> StartClient()
        {
            return await _client.ConnectToServer("127.0.0.1", 47000);
        }

        public async Task SetLogin(string login, string pass)
        {
            this.UserLogin = login;
            await this._client.WriteToServer(CommonData.LoginMessage + login + CommonData.CommonDataSeparator + pass);
        }

        public async Task SendMessage(string chatName, string message)
        {
            await this._client.WriteToServer(CommonData.ChatMessage + _chatKeys[chatName] + message);
        }

        private void _client_MessageEvent(string message)
        {
            if(_isLogged && message.StartsWith(CommonData.ChatMessage))
                MessageEvent?.Invoke(message.Substring(3), this._chatKeys.First(i => i.Value == int.Parse(message.Substring(2,1))).Key);
            else
            {
                if(message.StartsWith(CommonData.GetChatLIst))
                {
                    var counter = 1;
                    var chatList = message.Substring(2).Split(CommonData.CommonDataSeparator);
                    _chatKeys.Clear();
                    for (int i = 0; i < chatList.Length; i += 2)
                    {
                        string ch = chatList[i];
                        if (string.IsNullOrEmpty(ch))
                            continue;
                        _chatKeys.Add(ch, int.Parse(chatList[i + 1]));
                    }

                    _isLogged = true;

                    Messenger.Default.Send(AppMessage.LoginSequenceComplited);
                    Messenger.Default.Send(AppMessage.ChatListUpdated);
                }
                else if(message.StartsWith(CommonData.UpdateStateMessage))
                {
                    this._client.WriteToServer(CommonData.GetChatLIst);
                }
                else if (message.StartsWith(CommonData.GetUsersList))
                {
                    MessageEvent?.Invoke(message.Substring(2).Replace(CommonData.CommonDataSeparator, '\n'), "");
                }
                else if(message.StartsWith(CommonData.LoginMessage))
                {
                    Messenger.Default.Send(AppMessage.LoginFailed);
                }
            }
        }

        internal async Task GetUsersList()
        {
            await this._client.WriteToServer(CommonData.GetUsersList);
        }

        internal async Task CreateChatMessage(string args)
        {
            await this._client.WriteToServer(CommonData.CreateChatMessage + args);
        }
    }
}
