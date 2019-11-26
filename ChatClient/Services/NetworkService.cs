using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientServerLib;

namespace ChatClient.Services
{
    public class NetworkService
    {
        public delegate void MessageHandler(string message);
        //public delegate void NewRoomCreatedHandler(string message);
        public delegate void SignedInHandler();
        public event MessageHandler onMessageReceived;
        public event MessageHandler onNewRoomCreated;
        public event SignedInHandler onSignedIn;
        List<Room> rooms = new List<Room>();
        public List<Room> Rooms { get { return rooms; } }

        MainClient client;
        public NetworkService()
        {
            client = new MainClient();
            rooms.Add(new Room("General"));
            //client.onMessageReceived += (message) => onMessageReceived(message);
            client.onMessageReceived += HandleMessageFromServer;
            StartWork();
        }

        private void HandleMessageFromServer(string message)
        {
            if (message.StartsWith("/room"))
            {
                string[] rooms = message.Substring(6).Split('\n');
                foreach(string room in rooms)
                {
                    onNewRoomCreated?.Invoke(room);
                }
            }
            else if (message.StartsWith("/signed_in"))
            {
                onSignedIn();
            }
            else
            {
                onMessageReceived(message);
            }
        }

        private async Task StartWork()
        {
            await client.ConnectToServer("127.0.0.1", 14778);
        }

        public void SignIn(string login, string password)
        {
            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
                client.SendToServer($"/login {login} {password}");
        }

        public void Register(string login, string password)
        {
            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
                client.SendToServer($"/reg {login} {password}");
        }

        public void SendMessage(string message)
        {
            client.SendToServer(message);
        }

        public void ChatRoomSelected(string chatRoomName)
        {
            client.SendToServer($"/enter {chatRoomName}");
        }
    }
}
