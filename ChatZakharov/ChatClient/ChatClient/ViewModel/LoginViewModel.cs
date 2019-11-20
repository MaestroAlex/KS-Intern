using ChatClient.Interface;
using ChatClient.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TransitPackage;

namespace ChatClient.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        IFrameNavigationService navigation;

        private string login;
        public string Login
        {
            get => login;
            set
            {
                Set(ref login, value);
                LoginTryCommand.RaiseCanExecuteChanged();
            }
        }

        private string validationText;
        public string ValidationText
        {
            get => validationText;
            set => Set(ref validationText, value);
        }

        public LoginViewModel(IFrameNavigationService navigation)
        {
            this.navigation = navigation;
            LoginTryCommand = new RelayCommand<object>(LoginTryCommandExecute, LoginTryCommandCanExecute);
            PasswordChangedCommand = new RelayCommand(PasswordChangedCommandExecute);
            MainModel.Client.PropertyChanged += Client_PropertyChanged;
        }

        private void Client_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ConnectionState")
                Application.Current.Dispatcher
                    .BeginInvoke(new Action(LoginTryCommand.RaiseCanExecuteChanged));
        }

        #region Commands

        #region LoginTryCommand
        public RelayCommand<object> LoginTryCommand { get; private set; }

        private bool LoginTryCommandCanExecute(object obj)
        {
            return !string.IsNullOrWhiteSpace(Login)
                && !string.IsNullOrWhiteSpace((obj as PasswordBox)?.Password)
                && MainModel.Client.ConnectionState == ClientState.Connected;
        }

        private async void LoginTryCommandExecute(object obj)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] hashPassword =
                sha256.ComputeHash(Encoding.ASCII.GetBytes((obj as PasswordBox).Password));

            string hashPasswordString = Encoding.UTF8.GetString(hashPassword);

            //ActionEnum LoginResult = ActionEnum.ok;
            //MainModel.Client.ConnectionState = ClientState.LoggedIn;

            ActionEnum LoginResult = await Task.Run(() =>
                MainModel.Client.LoginActionRequest(Login, hashPasswordString));

            if (LoginResult == ActionEnum.wrong_pass)
                ValidationText = "Wrong password";


            else if (LoginResult == ActionEnum.bad_login)
            {
                ValidationText = "User doesn't exist";

                MetroWindow curWindow = Application.Current.MainWindow as MetroWindow;
                MessageDialogResult confirmCreateUser = await curWindow.ShowMessageAsync(
                     "Authentication",
                     "User doesn't exist, create ?",
                     MessageDialogStyle.AffirmativeAndNegative,
                     new MetroDialogSettings()
                     {
                         AffirmativeButtonText = "Create",
                         NegativeButtonText = "Cancel",
                     });

                if (confirmCreateUser == MessageDialogResult.Affirmative)
                {
                    ActionEnum createUser = await Task.Run(() => MainModel.Client.CreateNewUserActionRequest(Login, hashPasswordString));
                    if (createUser == ActionEnum.ok)
                    {
                        ValidationText = "";
                        navigation.NavigateTo("ChatsPage");
                    }
                }
            }

            else if (LoginResult == ActionEnum.ok)
            {
                ValidationText = "";
                navigation.NavigateTo("ChatsPage");
            }

        }
        #endregion

        #region LoginTryCommand
        public RelayCommand PasswordChangedCommand { get; private set; }

        private void PasswordChangedCommandExecute() => LoginTryCommand.RaiseCanExecuteChanged();
        #endregion

        #endregion
    }
}
