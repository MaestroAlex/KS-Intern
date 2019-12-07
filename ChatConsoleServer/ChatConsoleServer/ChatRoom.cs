using ChatHandler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatConsoleServer
{
    class ChatRoom
    {
        public static int ChatCounter = 1;
        private Dictionary<string, TcpClient> _ChatMembers = new Dictionary<string, TcpClient>();
        private int _ChatID;
        private string _Name;
        private string _ChatHistory = "";
        public int ChatID => _ChatID;
        public string Name => _Name;
        public Dictionary<string, TcpClient> ChatMembers => _ChatMembers;



        private DBManager DB = DBManager.GetInstance();

        public ChatRoom(int ID, string name)
        {
            _ChatID = ID;

            _Name = name;

            LoadHistory();
        }

        public ChatRoom(KeyValuePair<string, TcpClient> member1, KeyValuePair<string, TcpClient> member2)
        {
            _ChatID = ChatCounter;
            DB.InsertNewChat($"{member1.Key}:{member2.Key}",_ChatID);

            _Name = $"{member1.Key}:{member2.Key}";

            Console.WriteLine($"{MsgKeys.NewChat}|{_ChatID}|{_Name}");
 
            AddMember(member1.Key, member1.Value);
            AddMember(member2.Key, member2.Value);
            ChatCounter++;

        }

        public void ConnectMember(string name, TcpClient socket)
        {
            if (!_ChatMembers.ContainsKey(name))
            {
                _ChatMembers.Add(name, socket);
            }
            SendMessage($"{MsgKeys.ChatHistory}|{_ChatID}|{_ChatHistory}", socket);


        }

        public async void AddMember(string name, TcpClient socket)
        {
            DB.InsertUserToChat(name, ChatID).Wait();

            SendMessage($"{MsgKeys.NewChat}|{_ChatID}|{_Name}", socket);

            ConnectMember(name, socket);
        }

        public bool DisconnectMember(string name)
        {
            bool result = false;
            if (_ChatMembers.ContainsKey(name))
            {
                _ChatMembers.Remove(name);
                result = true;
            }
            return result;
        }

        private void LoadHistory()
        {
            Task.Run(async () =>
            {
                _ChatHistory = await DB.LoadChatHistory(ChatID);

            }).Wait();
        }

        public void Broadcast(string message, string sender = "", TcpClient socket = null)
        {
            message += MsgKeys.End;
            if (socket == null)
            {
                foreach (var member in _ChatMembers)
                {
                    SendMessage(message, member.Value);
                    _ChatHistory += message;
                }
                DBManager.GetInstance().InsertNewMessage(ChatID, sender, message);
            }
            else
            {
                SendMessage(message, socket);
            }
        }

        public void SendMessage(string message, TcpClient receiver)
        {
            if (!message.EndsWith(MsgKeys.End))
            {
                message += MsgKeys.End;
            }
            var byteSize = Encoding.Unicode.GetBytes(message).Count();

            byte[] buffer = Encoding.Unicode.GetBytes(message);
            receiver.GetStream().Write(buffer, 0, buffer.Length);
        }

        public bool ContainsMember(string memberName)
        {
            bool result = false;
            if (_ChatMembers.ContainsKey(memberName))
            {
                result = true;
            }
            return result;
        }

        public int MemberCount()
        {
            return _ChatMembers.Count;
        }
    }


}
