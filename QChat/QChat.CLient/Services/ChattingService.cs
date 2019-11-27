using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.CLient.Messaging;

namespace QChat.CLient.Services
{
    class ChattingService
    {
        private Dictionary<ulong, Chat> _roomChats = new Dictionary<ulong, Chat>();
        private Dictionary<ulong, Chat> _userChats = new Dictionary<ulong, Chat>();
        private Dictionary<ulong, Chat> _groupChats = new Dictionary<ulong, Chat>();

        public ChattingService()
        {

        }

        public Chat GetChat(ulong id, ChatType type)
        {
            switch (type)
            {
                case ChatType.RoomChat: return GetRoomChat(id);
                default: throw new NotImplementedException();
            }
        }
        private Chat GetRoomChat(ulong id)
        {
            _roomChats.TryGetValue(id, out var roomChat);
            return roomChat;
        }


        public Chat GetDeafultChat(ChatType type)
        {
            switch (type)
            {
                case ChatType.RoomChat: return GetDefaultRoomChat();
                default: throw new NotImplementedException();
            }
        }
        private Chat GetDefaultRoomChat() => _roomChats.FirstOrDefault().Value;
    }
}
