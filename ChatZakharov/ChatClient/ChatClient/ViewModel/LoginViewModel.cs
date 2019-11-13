using ChatClient.Interface;
using ChatClient.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatClient.ViewModel
{
    public class LoginViewModel : ViewModelBase, IDataErrorInfo
    {
        IFrameNavigationService navigation;

        private string name;
        public string Name
        {
            get => name;
            set
            {
                Set(ref name, value);
                LoginTryCommand.RaiseCanExecuteChanged();
            }
        }

        bool loginOperation = false;
        public LoginViewModel(IFrameNavigationService navigation)
        {
            this.navigation = navigation;
            LoginTryCommand = new RelayCommand<object>(LoginTryCommandExecute, LoginTryCommandCanExecute);
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

        private bool LoginTryCommandCanExecute(object arg)
        {
            return !string.IsNullOrWhiteSpace(Name)
                && MainModel.Client.ConnectionState == ClientState.Connected;
        }

        private async void LoginTryCommandExecute(object obj)
        {
            loginOperation = true;

            bool LoginResult = await Task.Run(() => MainModel.Client.LoginActionRequest(Name));
            if (!LoginResult)
                RaisePropertyChanged("Name"); // запуск валидации

            loginOperation = false;

            if (LoginResult)
                navigation.NavigateTo("ChatsPage");
        }
        #endregion

        #endregion

        #region Validation
        public string Error => null;
        public string this[string propertyName]
        {
            get => loginOperation ? "Such user exist" : null;
        }
        #endregion
    }
}
