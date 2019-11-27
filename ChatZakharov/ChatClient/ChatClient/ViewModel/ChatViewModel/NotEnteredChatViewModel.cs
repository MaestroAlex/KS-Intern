﻿using ChatClient.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Security.Cryptography;
using TransitPackage;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls;
using ChatClient.Views.ChatView;

namespace ChatClient.ViewModel.ChatViewModel
{
    public class NotEnteredChatViewModel : ViewModelBase
    {
        private ChannelType channelType;

        public ChannelType ChannelType
        {
            get => channelType;
            set => Set(ref channelType, value);
        }

        private string validationText;
        public string ValidationText
        {
            get => validationText;
            set => Set(ref validationText, value);
        }

        Channel channel;
        public NotEnteredChatViewModel(Channel channel)
        {
            this.channel = channel;
            ChannelType = channel.Type;
            EnterCommand = new RelayCommand<object>(EnterCommandExecute, EnterCommandCanExecute);
            PasswordChangedCommand = new RelayCommand(PasswordChangedCommandExecute);
        }


        #region Commands

        #region ChatsLoadedCommand
        public RelayCommand<object> EnterCommand { get; private set; }

        public bool EnterCommandCanExecute(object arg)
        {
            return (!string.IsNullOrWhiteSpace(((PasswordBox)arg)?.Password) 
                                            || ChannelType == ChannelType.public_open)
                && MainModel.Client.ConnectionState == ClientState.LoggedIn;
        }
        public async void EnterCommandExecute(object obj)
        {
            ValidationText = "";
            string hashPasswordString = null;

            if (!string.IsNullOrWhiteSpace((obj as PasswordBox).Password))
            {
                SHA256 sha256 = SHA256.Create();
                byte[] hashPassword =
                    sha256.ComputeHash(Encoding.ASCII.GetBytes((obj as PasswordBox).Password));

                hashPasswordString = Encoding.UTF8.GetString(hashPassword);
            }

            ActionEnum res = await Task.Run(() => MainModel.Client.EnterRoomActionRequest(channel, hashPasswordString));

            if (res == ActionEnum.wrong_pass)
                ValidationText = "Wrong password";

            else if (res == ActionEnum.bad)
                ValidationText = "Something gone wrong, try again later";

            else if (res == ActionEnum.ok)
            {
                ChatsViewModel chats = SimpleIoc.Default.GetInstance<ChatsViewModel>();
                HamburgerMenuItem curChat = chats.Channels.Where(channel => channel.Label == this.channel.Name).First();
                curChat.Tag = new CurrentChatView()
                {
                    DataContext = new CurrentChatViewModel(this.channel)
                };
                await Task.Run(() => MainModel.Client.GetRoomHistoryActionRequest(this.channel.Name));
            }
        }
        #endregion

        #region PasswordChanged
        public RelayCommand PasswordChangedCommand { get; private set; }

        private void PasswordChangedCommandExecute() => EnterCommand.RaiseCanExecuteChanged();
        #endregion

        #endregion
    }
}
