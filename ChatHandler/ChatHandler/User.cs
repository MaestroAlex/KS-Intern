using System.Net.Sockets;

namespace ChatHandler
{
    class User
    {
        string _Name;
        string _Password;
        TcpClient _tcpCLient;

        public void Connect(TcpClient socket, string name, string password)
        {
            _Name = name;
            _tcpCLient = socket;
            _Password = password;
        }

        public string Name { get => _Name; set => _Name = value; }
        public string Password { get => _Password; set => _Password = value; }
        public TcpClient tcpCLient { get => _tcpCLient; private set => _tcpCLient = value; }
    }
}
