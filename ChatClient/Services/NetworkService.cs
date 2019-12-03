using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientServerLib.ClientAndServer;
using ClientServerLib.Additional;
using ClientServerLib.Common;
using System.IO;

namespace ChatClient.Services
{
    public class NetworkService
    {
        public delegate void RoomHandler(string message);
        public delegate void MessageHandler(string userName, string message, string chatroomname);
        public delegate void SignedInHandler();
        public event RoomHandler onNewRoomCreated;
        public event RoomHandler onChangedRoom;
        public event MessageHandler onMessageReceived;
        public event SignedInHandler onSignedIn;

        MainClient client;
        public NetworkService()
        {
            client = new MainClient();
            client.onMessageReceived += HandleMessageFromServer;
            StartWork();
        }

        private async void HandleMessageFromServer(string message)
        {
            if (message.StartsWith(ChatSyntax.CreateRoomCmd))
            {
                string[] rooms = message.Substring(6).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(string room in rooms)
                {
                    onNewRoomCreated?.Invoke(room);
                }
            }
            else if (message.StartsWith(ChatSyntax.SignedInSignalCmd))
            {
                onSignedIn();
            }
            else if (message.StartsWith(ChatSyntax.EnterRoomCmd))
            {
                string[] roomName = message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                onChangedRoom(roomName[1]);
            }
            else if (message.StartsWith(ChatSyntax.ChatHistoryCmd))
            {
                string roomName = message.Substring(ChatSyntax.ChatHistoryCmd.Length + 1, message.IndexOf('\n') - (ChatSyntax.ChatHistoryCmd.Length + 1));
                message = message.Substring(message.IndexOf('\n') + 1);
                foreach(string msg in message.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] SenderMessage = msg.Split(new string[] { ChatSyntax.MessageDiv }, StringSplitOptions.RemoveEmptyEntries);
                    onMessageReceived(SenderMessage[0], SenderMessage[1], roomName);
                }
            }
            else
            {
                string[] SenderMessageRoom = message.Split(new string[] { ChatSyntax.MessageDiv }, StringSplitOptions.None);
                if (SenderMessageRoom.Length != 3)
                    return;
                onMessageReceived(SenderMessageRoom[0], SenderMessageRoom[1], SenderMessageRoom[2]);
            }
        }

        public async Task SendFile(string file)
        {
            byte[] fileBytes = File.ReadAllBytes(file);
            string readFile = Convert.ToBase64String(fileBytes);
            await client.SendToServer(ChatSyntax.ImageDiv + readFile);
        }

        private async Task StartWork()
        {
            await client.ConnectToServer("127.0.0.1", 14778);
        }

        public void SignIn(string login, string password)
        {
            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
                client.SendToServer($"{ChatSyntax.LoginCmd} {login} {password}");
        }

        public void Register(string login, string password)
        {
            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
                client.SendToServer($"{ChatSyntax.RegCmd} {login} {password}");
        }

        public void SendMessage(string message)
        {
            client.SendToServer(message);
        }

        public void ChatRoomSelected(string chatRoomName)
        {
            client.SendToServer($"{ChatSyntax.EnterRoomCmd} {chatRoomName}");
        }
    }
}
