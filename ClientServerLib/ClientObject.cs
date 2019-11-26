using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ClientServerLib
{
    public class ClientObject
    {
        private string userLogin;
        readonly TcpClient socket;
        Room chatRoom;
        public ClientObject(TcpClient socket)
        {
            this.socket = socket;
        }

        public void SetLogin(string login)
        {
            this.userLogin = login;
        }

        public Room ChatRoom { get { return chatRoom; } set { chatRoom = value; } }
        public TcpClient Socket { get { return socket; } }
        public string UserLogin { get { return  userLogin; } }
    }
}
