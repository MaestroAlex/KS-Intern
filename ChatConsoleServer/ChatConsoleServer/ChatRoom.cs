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
        private static int ChatCounter = 0;
        private Dictionary<string, Socket> _ChatMembers = new Dictionary<string, Socket>();
        private int _ID;

        public int ID => _ID;
        public Dictionary<string, Socket> ChatMembers => _ChatMembers;

        public ChatRoom()
        {
            _ID = ChatCounter;
            ChatCounter++;
        }

        public ChatRoom(KeyValuePair<string, Socket> member1, KeyValuePair<string, Socket> member2)
        {

            AddMember(member1.Key, member1.Value);
            AddMember(member2.Key, member2.Value);

            _ID = ChatCounter;

            Broadcast($"{MsgKeys.NewChat}|{_ID}|{member1.Key}", member2.Value);

            Broadcast($"{MsgKeys.NewChat}|{_ID}|{member2.Key}", member1.Value);
            ChatCounter++;

        }
        public void AddMember(string name, Socket socket)
        {
            if (!_ChatMembers.ContainsKey(name))
            {
                _ChatMembers.Add(name, socket);
            }

            //Broadcast($"{MsgKeys.JoinedRoom}|{name}");
        }

        public bool RemoveMember(string name)
        {
            bool result = false;
            if (ChatMembers.ContainsKey(name))
            {
                ChatMembers.Remove(name);
                result = true;
            }
            return result;
        }

        public void Broadcast(string message, Socket socket = null)
        {
            if (socket == null)
            {
                foreach (var member in _ChatMembers)
                {
                    SendMessage(message, member.Value);
                }
            }
            else
            {
                SendMessage(message, socket);
            }
            //Console.WriteLine($"{DateTime.Now.ToString()}|{_ClientName }:{message}");
        }

        private void SendMessage(string message, Socket receiver)
        {
            byte[] buffer = null;

            message = message + "$";

            buffer = Encoding.Unicode.GetBytes(message);

            receiver.Send(buffer);
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
