using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ClientServerLib.Common
{
    public class ClientObject
    {
        int id;
        string userLogin;
        readonly TcpClient socket;
        ChatRoom activeChatRoom;
        List<ChatRoom> clientsChatRooms;
        public ClientObject(TcpClient socket)
        {
            this.socket = socket;
            clientsChatRooms = new List<ChatRoom>();
        }

        public void Login(int id, string login)
        {
            this.id = id;
            this.userLogin = login;
        }

        public void SetActiveChatRoom(ChatRoom newRoom)
        {
            activeChatRoom = newRoom;
        }

        
        public List<ChatRoom> ClientsChatRooms { get { return clientsChatRooms; } }
        public int Id { get { return id; } }
        public ChatRoom ActiveChatRoom { get { return activeChatRoom; } }
        public TcpClient Socket { get { return socket; } }
        public string UserLogin { get { return  userLogin; } }
    }
}
