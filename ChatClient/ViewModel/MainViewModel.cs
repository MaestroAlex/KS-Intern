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
using System;
using System.Text;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media.Imaging;

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

        ~MainViewModel()
        {
            Directory.Delete(ChatSyntax.ResourcesDir, true);
        }

        private void NewRoomCreated(string roomName)
        {
            Rooms.Add(new ChatRoom(roomName));
            MessagesInChats.Add(roomName, new ObservableCollection<MessageItem>());
            if (MessageHistory == null)
                ActiveRoomChanged(roomName);
        }

        internal async Task SendFiles(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    await netServices.SendFile(file);
                }
            }
        }

        string message;
        public string Message
        {
            get { return message; }
            set { Set(ref message, value); }
        }

        private async void ReceiveMessage(string userName, string message, string roomName)
        {
            if (message.StartsWith(ChatSyntax.ImageDiv))
            {
                BitmapImage image;
                try
                {
                    image = await GetImageFromMessage(message);
                }
                catch { image = null; }
                MessagesInChats[roomName].Add(new MessageItem(userName, image));
            }
            else
                MessagesInChats[roomName].Add(new MessageItem(userName, message));
        }

        Random rnd = new Random();

        private async Task<BitmapImage> GetImageFromMessage(string message)
        {
            Directory.CreateDirectory(ChatSyntax.ResourcesDir);
            message = message.Substring(ChatSyntax.ImageDiv.Length);
            byte[] imageBytes = Convert.FromBase64String(message);
            var time = DateTime.Now;
            
            string filePath = ChatSyntax.ResourcesDir + "\\file-" + time.ToString("hh_mm_ss_dd_MM_yyyy") + rnd.Next(100000) + ".bmp";
            File.WriteAllBytes(filePath, imageBytes);

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(filePath, UriKind.Relative);
            image.EndInit();
            image.Freeze();
            while (image.IsDownloading)
                Thread.Sleep(100);
            return image;
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