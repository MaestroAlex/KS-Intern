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
        private static ChatRoom GeneralChat = new ChatRoom();
        private static List<ChatRoom> PrivateChats = new List<ChatRoom>();
        //private static Dictionary<string, Socket> _ClientsList = new Dictionary<string, Socket>();

        private readonly Socket _Listener;
        private readonly string _ClientName;

        public HandleClient(Socket listener, string clientName)
        {
            _Listener = listener;
            _ClientName = clientName;
            GeneralChat.ChatMembers.Add(clientName, listener);
            GeneralChat.AddMember(clientName, listener);
            //Broadcast($"{clientName} Joined the room.");
            Console.WriteLine($"{clientName} Joined the room.");
            Task.Factory.StartNew(StartListening);
        }

        ~HandleClient() { }

        /*public void Broadcast(string message, Socket receiver = null)
        {
            if (receiver == null)
            {
                foreach (var client in _ClientsList)
                {
                    if (client.Value != _Listener)
                    {
                        SendMessage(message, client.Value);
                    }
                }
            }
            else
            {
                SendMessage(message, receiver);
            }
            Console.WriteLine($"{DateTime.Now.ToString()}|{_ClientName }:{message}");
        }

        private void SendMessage(string message, Socket receiver)
        {
            byte[] buffer = null;

            message = message + "$";

            buffer = Encoding.Unicode.GetBytes(message);

            receiver.Send(buffer);
        }
        */

        private void ParseMessage(ref string message)
        {
            message = message.Substring(0, message.LastIndexOf("$"));

            if (message.StartsWith(MsgKeys.NewChat))
            {
                var messageData = message.Split('|');
                var receiverName = messageData[1];
                receiverName = receiverName.Replace(" ", "");
                if (IsClientExists(receiverName, GeneralChat))
                {
                    var chat = GetChat(_ClientName, receiverName);
                    if (chat != null)
                    {
                        chat.Broadcast($"Personal chat with {receiverName} already exists.", _Listener);
                    }
                    else
                    {
                        chat = new ChatRoom(
                            new KeyValuePair<string, Socket>(receiverName, GeneralChat.ChatMembers[receiverName]),
                            new KeyValuePair<string, Socket>(_ClientName, _Listener));

                        PrivateChats.Add(chat);
                    }
                }
                else
                {
                    GeneralChat.Broadcast($"{MsgKeys.ServerAnswer}|Client {receiverName} doesn't exist", _Listener);
                }
                return;
            }

            if (message.StartsWith(MsgKeys.ToChat))
            {
                var messageData = message.Split('|');
                var chatID = int.Parse(messageData[2]);
                //message = messageData[3];
                var chat = GetChat(chatID);
                if (chat != null)
                {
                    chat.Broadcast(message);
                    //Broadcast(msg, _PersonalChats[receiverName]);
                }
                else
                {
                    GeneralChat.Broadcast($"Chat {chatID} is not available", _Listener);
                }
                return;
            }

            GeneralChat.Broadcast(message);
        }

        private bool IsClientExists(string clientName, ChatRoom chat)
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
                if (chat.ID == chatID)
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
                    var bytes = _Listener.Available;
                    byte[] buffer = new byte[256];
                    string data = null;
                    _Listener.Receive(buffer);

                    data = Encoding.Unicode.GetString(buffer);

                    ParseMessage(ref data);

                    var message = DateTime.Now.ToString() + "|" + _ClientName + " :" + data;
                    Console.WriteLine(message);
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.ToString());
                    Console.WriteLine(_ClientName + " disconnected");
                    GeneralChat.RemoveMember(_ClientName);
                    foreach (var chat in PrivateChats)
                    {
                        chat.RemoveMember(_ClientName);
                    }

                    _Listener.Close();
                    break;
                }
            }
        }
    }
    #endregion


}
