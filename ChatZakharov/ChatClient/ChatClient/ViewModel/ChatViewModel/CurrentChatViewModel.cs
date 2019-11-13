using ChatClient.Converters;
using ChatClient.Interface;
using ChatClient.Models;
using ChatClient.Utility.UIMessage;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TransitPackage;

namespace ChatClient.ViewModel.ChatViewModel
{
    class CurrentChatViewModel : ViewModelBase
    {
        public string UsernameSelf { get; private set; }
        public string UsernameDestination { get; private set; }

        public TextBox CurrentMessage { get; set; }
        public Grid MessagesHistoryGrid { get; set; }

        public CurrentChatViewModel(string username)
        {
            NewLineCommand = new RelayCommand<object>(NewLineCommandExecute, NewLineCommandCanExecute);
            SendMessageCommand = new RelayCommand<object>(SendMessageCommandExecute, SendMessageCommandCanExecute);
            UsernameDestination = username;
            UsernameSelf = MainModel.Client.Name;
            MainModel.Client.MessageReceived += Client_MessageReceived;
        }

        private void Client_MessageReceived(object sender, Message e)
        {
            if (e.From == UsernameDestination)
                Application.Current.Dispatcher
                    .BeginInvoke(new Action<string>(ReceiveMessage), e.Text);
        }

        private void ReceiveMessage(string messageText)
        {
            UIMessageInput Message = new UIMessageInput(new Message
            {
                From = UsernameDestination,
                To = UsernameSelf,
                Text = messageText
            });
            Message.CreateUIMessage();
            BindNewMessageToGrid(Message);
        }

        #region Commands

        #region NewLineCommand
        public RelayCommand<object> NewLineCommand { get; private set; }

        private bool NewLineCommandCanExecute(object arg)
        {
            return true;
        }

        private void NewLineCommandExecute(object obj)
        {
            CurrentMessage.Text += "\r\n";
            CurrentMessage.CaretIndex = CurrentMessage.Text.Length;
        }
        #endregion

        #region SendMessageCommand
        public RelayCommand<object> SendMessageCommand { get; private set; }

        private bool SendMessageCommandCanExecute(object arg)
        {
            return !string.IsNullOrWhiteSpace(CurrentMessage.Text);
        }

        private async void SendMessageCommandExecute(object obj)
        {
            string currentMesageText = CurrentMessage.Text;

            bool res = await Task.Run(() =>
                MainModel.Client.SendMessageActionRequest(new Message()
                {
                    From = UsernameSelf,
                    To = UsernameDestination,
                    Text = currentMesageText
                }));

            if (res)
                SendUIMessage();
        }

        private void SendUIMessage()
        {
            UIMessageOutput Message = new UIMessageOutput(new Message
            {
                From = UsernameSelf,
                To = UsernameDestination,
                Text = CurrentMessage.Text
            });
            Message.CreateUIMessage();
            BindNewMessageToGrid(Message);
            ClearCurrentMessage();
        }

        private void BindNewMessageToGrid(UIMessage Message)
        {
            try
            {
                Binding messageWidthBinding = new Binding("ActualWidth")
                {
                    Source = MessagesHistoryGrid,
                    Converter = new WindowWidthToMessageWidthConverter(),
                    ConverterParameter = 1.3,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                Message.NewMessage.SetBinding(TextBlock.MaxWidthProperty, messageWidthBinding);

                RowDefinition newMessageRow = new RowDefinition
                {
                    Height = GridLength.Auto
                };

                MessagesHistoryGrid.RowDefinitions.Add(newMessageRow);
                Grid.SetRow(Message.ResultMessageBorder, MessagesHistoryGrid.RowDefinitions.Count - 1);
                MessagesHistoryGrid.Children.Add(Message.ResultMessageBorder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ClearCurrentMessage()
        {
            CurrentMessage.Text = "";
            CurrentMessage.CaretIndex = CurrentMessage.Text.Length;
        }
        #endregion

        #endregion
    }
}
