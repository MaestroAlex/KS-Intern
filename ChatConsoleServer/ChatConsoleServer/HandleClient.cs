using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using ChatHandler;
using System.Collections.Generic;

namespace ChatConsoleServer
{

    #region DLL
    public class HandleClient
    {
        private static ChatRoom GeneralChat = new ChatRoom(1, "General");
        private static List<ChatRoom> PrivateChats = new List<ChatRoom>();
        private DBManager DB = DBManager.GetInstance();
        private List<int> _ClientsChatsID = new List<int>();

        private readonly TcpClient _Listener;
        private readonly string _ClientName;
        private readonly string _Password;

        public TcpClient Listener => _Listener;

        public HandleClient(TcpClient listener, string clientName, string password, bool newClient = true)
        {
            _Listener = listener;
            _ClientName = clientName;
            _Password = password;

            var buffer = Encoding.Unicode.GetBytes($"{MsgKeys.LogIn}|{clientName}{MsgKeys.End}");
            listener.GetStream().Write(buffer, 0, buffer.Length);

            if (newClient)
            {
                DBManager.GetInstance().RegisterNewUser(clientName, password).Wait();
                GeneralChat.AddMember(clientName, listener);
            }
            else
            {
                GeneralChat.ConnectMember(clientName, listener);

                Task.Run(async () => _ClientsChatsID = await DB.GetUsersChats(_ClientName)).Wait();

                ConnectUserToChats();
            }

            Console.WriteLine($"{clientName} Joined the room.");
            Task.Factory.StartNew(StartListening);
        }

        ~HandleClient() { }

        private void ParseMessage(ref string message)
        {
            message = message.Substring(0, message.LastIndexOf(MsgKeys.End));

            if (message.StartsWith(MsgKeys.NewChat))
            {
                var messageData = message.Split('|');
                var receiverName = messageData[1];
                receiverName = receiverName.Replace(" ", "");
                if (IsClientOnline(receiverName, GeneralChat))
                {
                    var chat = GetChat(_ClientName, receiverName);
                    if (chat != null)
                    {
                        chat.SendMessage($"Personal chat with {receiverName} already exists.", _Listener);
                    }
                    else
                    {
                        chat = new ChatRoom(
                            new KeyValuePair<string, TcpClient>(receiverName, GeneralChat.ChatMembers[receiverName]),
                            new KeyValuePair<string, TcpClient>(_ClientName, _Listener));

                        PrivateChats.Add(chat);
                    }
                }
                else
                {
                    GeneralChat.SendMessage($"{MsgKeys.ServerAnswer}|Client {receiverName} doesn't exist", _Listener);
                }
                return;
            }

            if (message.StartsWith(MsgKeys.ToChat))
            {
                var messageData = message.Split('|');
                var chatID = int.Parse(messageData[2]);
                var chatName = messageData[3];
                //message = messageData[4];
                var chat = GetChat(chatID);
                if (chat != null)
                {
                    chat.Broadcast(message, _ClientName);
                }
                else
                {
                    GeneralChat.SendMessage($"Chat {chatID} is not available", _Listener);
                }
                return;
            }

            if (message.StartsWith(MsgKeys.GeneralChat))
            {
                var messageData = message.Split('|');
                GeneralChat.Broadcast(message, _ClientName);
            }

        }

        private void ConnectUserToChats()
        {
            foreach (var id in _ClientsChatsID)
            {
                foreach (var chat in PrivateChats)
                {
                    if (chat.ChatID == id)
                    {
                        chat.ConnectMember(_ClientName, _Listener);
                        Console.WriteLine($"Connect {_ClientName}");
                    }
                }
            }
        }

        private bool IsClientOnline(string clientName, ChatRoom chat)
        {
            bool result = false;
            foreach (var client in chat.ChatMembers)
            {
                if (client.Key.Equals(clientName))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        private ChatRoom GetChat(int chatID)
        {
            ChatRoom result = null;
            foreach (var chat in PrivateChats)
            {
                if (chat.ChatID == chatID)
                {
                    result = chat;
                    break;
                }
            }
            return result;
        }

        private ChatRoom GetChat(params string[] membersNames)
        {
            ChatRoom result = null;
            foreach (var chat in PrivateChats)
            {
                var chatSize = chat.MemberCount();
                foreach (var member in membersNames)
                {
                    if (chat.ContainsMember(member))
                    {
                        chatSize--;
                    }
                    else
                    {
                        chatSize++;
                    }
                }
                if (chatSize == 0)
                {
                    result = chat;
                    break;
                }

            }
            return result;
        }

        private void StartListening()
        {
            while (_Listener.Connected)
            {
                try
                {

                    var clientStream = _Listener.GetStream();

                    var size = _Listener.ReceiveBufferSize;
                    byte[] buffer = new byte[size];
                    string data = null;

                    clientStream.Read(buffer, 0, (int)size);
                    data = Encoding.Unicode.GetString(buffer);

                    ParseMessage(ref data);

                    var message = DateTime.Now.ToString() + "|" + _ClientName + " :" + data;
                    Console.WriteLine(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(_ClientName + " disconnected");
                    GeneralChat.DisconnectMember(_ClientName);
                    foreach (var chat in PrivateChats)
                    {
                        chat.DisconnectMember(_ClientName);
                    }

                    _Listener.Close();
                    break;
                }
            }
        }

        public static async Task FillChatsAsync()
        {
            var chats = await DBManager.GetInstance().GetChats();
            int lastID = 0;
            foreach (var chat in chats)
            {
                if (chat.Key > 1)
                {
                    if (chat.Key > lastID)
                        lastID = chat.Key;
                    PrivateChats.Add(new ChatRoom(chat.Key, chat.Value));
                }
            }
            ChatRoom.ChatCounter = lastID+1;
        }

        public static string[] ParseLogin(string message)
        {
            string[] result = null;
            if (message.StartsWith(MsgKeys.LogIn))
            {
                try
                {
                    result = message.Split('|');
                }
                catch (Exception e) { }

            }
            return result;
        }
    }
    #endregion


}
