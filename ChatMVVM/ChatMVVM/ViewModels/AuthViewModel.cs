using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatMVVM.Annotations;
using ChatMVVM.Models;
using ChatMVVM.ViewModels.Commands;

namespace ChatMVVM.ViewModels
{
    struct User
    {
        public string Name;
        public Socket Socket;
    }

    class AuthViewModel : INotifyPropertyChanged
    {
        public ICommand LogIn => new DelegateCommand(ConnectUser, (obj) => _User.Name != null && _User.Name != "");

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public static User _User;

        public string UserName
        {
            get
            {
                return _User.Name;
            }
            set
            {
                _User.Name = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public void ConnectUser(object obj)
        {
            if (_User.Name != null && _User.Name != "")
            {
                try
                {
                    _User.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _User.Socket.Connect("127.0.0.1", 904);
                    _User.Socket.Send(Encoding.Unicode.GetBytes(UserName + "$"));
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
                var chatWindow = new ChatWindow();
                chatWindow.Show();
            }
            return;
        }
    }
}
