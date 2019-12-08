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
    class Chat
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

        public Chat(int ID, string name)
        {
            _ChatID = ID;

            _Name = name;

            LoadHistory();
        }

        public Chat(KeyValuePair<string, TcpClient> member1, KeyValuePair<string, TcpClient> member2)
        {
            _ChatID = ChatCounter;
            DB.InsertNewChat($"{member1.Key}:{member2.Key}", _ChatID).Wait();

            _Name = $"{member1.Key}:{member2.Key}";

            Console.WriteLine($"{MsgKeys.NewChat}|{_ChatID}|{_Name}");

            AddMember(member1.Key, member1.Value);
            AddMember(member2.Key, member2.Value);
            ChatCounter++;

        }

        public async void SendHistory(TcpClient socket)
        {
            SendMessage($"{MsgKeys.ChatHistory}|{_ChatID}|{_ChatHistory}", socket).Wait();
        }

        public async void ConnectMember(string name, TcpClient socket)
        {
            if (!_ChatMembers.ContainsKey(name))
            {
                _ChatMembers.Add(name, socket);
            }
            await SendMessage($"{MsgKeys.ChatHistory}|{_ChatID}|{_ChatHistory}", socket);


        }

        public async void AddMember(string name, TcpClient socket)
        {
            await DB.InsertUserToChat(name, ChatID);

            SendMessage($"{MsgKeys.NewChat}|{_ChatID}|{_Name}", socket).Wait();

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
                /*if (!string.IsNullOrWhiteSpace(_ChatHistory))
                {
                    //_ChatHistory = _ChatHistory.Substring(0,_ChatHistory.LastIndexOf(MsgKeys.End));
                    _ChatHistory = await Encrypt.DecodeMessage(_ChatHistory);
                }
                */
            }).Wait();
        }

        public async void Broadcast(string message, string sender)
        {
            foreach (var member in _ChatMembers)
            {
                SendMessage(message, member.Value, true, ChatID, sender).Wait();
                LoadHistory();
                //_ChatHistory = await DBManager.GetInstance().LoadChatHistory(_ChatID);
            }

        }

        public static async Task SendMessage(string message, TcpClient receiver, bool insertToDataBase = false, int chatID = 0, string sender = "")
        {
            message += MsgKeys.End;
            message = await Encrypt.EncodeMessage(message);

            if (insertToDataBase)
            {
                DBManager.GetInstance().InsertNewMessage(chatID, sender, message).Wait();
            }

            byte[] buffer = Encoding.Unicode.GetBytes(message + MsgKeys.End);
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
