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
        public ICommand LogIn => new DelegateCommand(ConnectUser, (obj) =>
        {
            bool result = true;
            if ((string.IsNullOrEmpty(Username) ||
                string.IsNullOrEmpty(Password)))
            {
                result = false;
            }
            return result;
        });

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private ClientChatHandler _ChatHandler = ClientChatHandler.Instance();

        public string Username
        {
            get => _ChatHandler.Username;
            set
            {
                _ChatHandler.Username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Password
        {
            get => _ChatHandler.Password;
            set
            {
                _ChatHandler.Password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public async void ConnectUser(object obj)
        {
           /* Task.Run(async()=>
            {*/
                if (await _ChatHandler.ConnectUser())
                {
                    MainWindowModel.GetInstance().setChatPage();
                }
            //}).Wait();
            
        }
    }
}
