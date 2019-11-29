using ChatClient.Converters;
using ChatClient.Interface;
using ChatClient.Models;
using ChatClient.Utility.UIMessage;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.IO;
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
        public Channel ChannelDestination { get; private set; }

        public TextBox CurrentMessage { get; set; }
        public Grid MessagesHistoryGrid { get; set; }

        public CurrentChatViewModel(Channel channel)
        {
            NewLineCommand = new RelayCommand<object>(NewLineCommandExecute, NewLineCommandCanExecute);
            SendMessageCommand = new RelayCommand<object>(SendMessageCommandExecute, SendMessageCommandCanExecute);
            DropCommand = new RelayCommand<DragEventArgs>(DropCommandExecute);
            ChannelDestination = channel;
            UsernameSelf = MainModel.Client.Name;
            MainModel.Client.MessageReceived += Client_MessageReceived;
        }

        private void Client_MessageReceived(object sender, Message e)
        {
            if (e.ChatDestination == ChannelDestination.Name)
                Application.Current.Dispatcher
                    .BeginInvoke(new Action<Message>(ReceiveMessage), e);
        }

        private void ReceiveMessage(Message receivedMessage)
        {
            UIMessage UIMessage = null;

            if (receivedMessage.From == UsernameSelf) // когда загружаем историю
                UIMessage = new UIMessageOutput(receivedMessage);
            else
                UIMessage = new UIMessageInput(receivedMessage);

            UIMessage.CreateUIMessage();
            BindNewMessageToGrid(UIMessage);
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
            Message sentMessage = new Message()
            {
                From = UsernameSelf, // если отправляем пользователю, то в чат себя у него, если в команту, то в чат комнаты у участников комнаты
                ChatDestination = ChannelDestination.Type == ChannelType.user ? UsernameSelf : ChannelDestination.Name,
                To = ChannelDestination.Name,
                MessageType = MessageType.text,
                Content = CurrentMessage.Text,
                Date = DateTime.Now
            };

            bool res = await Task.Run(() => MainModel.Client.SendMessageActionRequest(sentMessage));

            if (res)
                SendUIMessage(sentMessage);
        }

        private void SendUIMessage(Message sentMessage)
        {
            UIMessageOutput Message = new UIMessageOutput(sentMessage);
            Message.CreateUIMessage();
            BindNewMessageToGrid(Message);
            ClearCurrentMessage();
        }

        private void BindNewMessageToGrid(UIMessage uiMessage)
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

                uiMessage.Content.SetBinding(FrameworkElement.MaxWidthProperty, messageWidthBinding);

                RowDefinition newMessageRow = new RowDefinition
                {
                    Height = GridLength.Auto
                };

                MessagesHistoryGrid.RowDefinitions.Add(newMessageRow);
                Grid.SetRow(uiMessage.ResultMessageBorder, MessagesHistoryGrid.RowDefinitions.Count - 1);
                MessagesHistoryGrid.Children.Add(uiMessage.ResultMessageBorder);
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

        #region DropCommand
        public RelayCommand<DragEventArgs> DropCommand { get; private set; }

        public async void DropCommandExecute(DragEventArgs e)
        {
            IDataObject data = e.Data;
            string filePath = ((string[])e.Data.GetData("FileDrop")).First();
            byte[] blob = null;
            try
            {
                blob = File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("drag n drop exception");
            }

            if (blob != null)
            {
                Message sentMessage = new Message()
                {
                    From = UsernameSelf, // если отправляем пользователю, то в чат себя у него, если в команту, то в чат комнаты у участников комнаты
                    ChatDestination = ChannelDestination.Type == ChannelType.user ? UsernameSelf : ChannelDestination.Name,
                    To = ChannelDestination.Name,
                    MessageType = DefineType(filePath),
                    Content = Convert.ToBase64String(blob),
                    Date = DateTime.Now
                };

                bool res = await Task.Run(() => MainModel.Client.SendMessageActionRequest(sentMessage));

                if (res)
                    SendUIMessage(sentMessage);
            }

        }

        private static string[] imageExtensions = new string[]
        {
            ".jpg", ".jpeg", ".jfif", ".jpeg 2000",
            ".exif", ".tiff", ".gif", ".bmp", ".png"
        };
        private MessageType DefineType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            if (imageExtensions.Contains(ext))
                return MessageType.image;
            else
                return MessageType.document;
        }
        #endregion

        #endregion
    }
}
