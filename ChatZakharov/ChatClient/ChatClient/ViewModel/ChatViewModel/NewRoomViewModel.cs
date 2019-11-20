using ChatClient.Models;
using ChatClient.Views.ChatView;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TransitPackage;

namespace ChatClient.ViewModel.ChatViewModel
{
    public class NewRoomViewModel : ViewModelBase
    {
        private string name;
        public string Name
        {
            get => name;
            set
            {
                Set(ref name, value);
                CreateRoomCommand.RaiseCanExecuteChanged();
            }
        }

        private string validationText;
        public string ValidationText
        {
            get => validationText;
            set => Set(ref validationText, value);
        }

        HamburgerMenuItemCollection channels;
        public NewRoomViewModel(HamburgerMenuItemCollection channels)
        {
            this.channels = channels;
            CreateRoomCommand = new RelayCommand<object>(CreateRoomCommandExecute, CreateRoomCommandCanExecute);
        }
        #region Commands

        public RelayCommand<object> CreateRoomCommand { get; private set; }

        private bool CreateRoomCommandCanExecute(object arg)
        {
            return !string.IsNullOrWhiteSpace(Name)
                && MainModel.Client.ConnectionState == ClientState.LoggedIn;
        }

        private async void CreateRoomCommandExecute(object obj)
        {
            ValidationText = "";
            string hashPasswordString = null;

            if (!string.IsNullOrWhiteSpace((obj as PasswordBox).Password))
            {
                SHA256 sha256 = SHA256.Create();
                byte[] hashPassword =
                    sha256.ComputeHash(Encoding.ASCII.GetBytes((obj as PasswordBox).Password));

                hashPasswordString = Encoding.UTF8.GetString(hashPassword);
            }

            ActionEnum res = await Task.Run(() =>
                MainModel.Client.CreateNewRoomActionRequest(Name, hashPasswordString));

            // if res == ok => server sends new room

            if (res == ActionEnum.room_exist)
                ValidationText = "Such room exist";

            else if (res == ActionEnum.bad)
                ValidationText = "Something gone wrong, try again later";
        }

        #endregion

    }
}
