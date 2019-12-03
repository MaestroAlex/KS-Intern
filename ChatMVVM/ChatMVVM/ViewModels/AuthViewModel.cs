using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatMVVM.Annotations;
using ChatMVVM.ViewModels.Commands;
using ChatHandler;

namespace ChatMVVM.ViewModels
{

    class AuthViewModel : INotifyPropertyChanged
    {
        public ICommand LogIn => new DelegateCommand(ConnectUser, (obj) => !string.IsNullOrEmpty(_ChatHandler.UserName));

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private ClientChatHandler _ChatHandler = ClientChatHandler.Instance();        

        public string UserName
        {
            get
            {
                return _ChatHandler.UserName;
            }
            set
            {
                _ChatHandler.UserName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public async void ConnectUser(object obj)
        {
            await _ChatHandler.ConnectUser(UserName);
            MainWindowModel.GetInstance().setChatPage();
        }
    }
}
