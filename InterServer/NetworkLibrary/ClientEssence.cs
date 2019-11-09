
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLibrary
{
    public class ClientEssence
    {
        private readonly TcpClient _socket;
        private string _userLogin;
        private uint _userId;
        public ClientEssence(TcpClient socket)
        {
            this._socket = socket;
        }

        public void DoLogin(string login)
        {
            this._userLogin = login;
            _userId = (uint)new Random().Next(0, 100000);
        }
        public TcpClient Socket
        {
            get => this._socket;
        }

        public string UserLogin
        {
            get => this._userLogin;
        }
        public uint UserId
        {
            get => _userId;
        }
    }
}
