using ChatClient.Interface;
using ChatClient.Models;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChatClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        IFrameNavigationService navigation;

        private string title = "Simple Chat";
        public string Title
        {
            get => title;
            set => Set(ref title, value);
        }

        private ClientState connectionState = ClientState.LoggedOut;
        public ClientState ConnectionState
        {
            get => connectionState;
            set
            {
                Set(ref connectionState, value);
                TitleChange();
                Application.Current.Dispatcher
                    .BeginInvoke(new Action(ReLoginCommand.RaiseCanExecuteChanged));
            }
        }

        public RelayCommand MainWindowLoaded { get; private set; }
        public MainViewModel(IFrameNavigationService navigation)
        {
            this.navigation = navigation;
            MainWindowLoaded = new RelayCommand(() =>
            {
                //при дебаге с брейкпоинтами не работает почему-то
                //SimpleIoc.Default.GetInstance<IFrameNavigationService>().MainFrame =
                //LogicalTreeHelper.FindLogicalNode(Application.Current.MainWindow, "MainFrame") as Frame;
                //navigation.NavigateTo("LoginPage");
            });

            ReLoginCommand = new RelayCommand<object>(ReLoginCommandExecute, ReLoginCommandCanExecute);
            ReconnectCommand = new RelayCommand<object>(ReconnectCommandExecute, ReconnectCommandCanExecute);
            MainModel.Client.PropertyChanged += Client_PropertyChanged;
            Connect();
            //GetConnectionChekInterval();
        }

        private void Client_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ConnectionState = MainModel.Client.ConnectionState;

            Application.Current.Dispatcher
                .BeginInvoke(new Action(ReconnectCommand.RaiseCanExecuteChanged));

            if (ConnectionState == ClientState.Disconnected ||
                ConnectionState == ClientState.LoggedOut)
                Application.Current.Dispatcher
                    .BeginInvoke(new Action<string>(navigation.NavigateTo), "LoginPage");
        }

        private async Task<bool> Connect()
        {
            if (await Task.Run(() => MainModel.Client.Connect()) &&
                   await Task.Run(() => MainModel.Client.GetConnectionChekInterval()) &&
                   await Task.Run(() => MainModel.Client.AESHandshakeWithRSAActionRequest()))
                return true;
            else
            {
                await Task.Run(() => MainModel.Client.LogoutActionRequest());
                return false;
            }
        }

        private void TitleChange()
        {
            if (ConnectionState == ClientState.LoggedIn)
                Title = "Simple Chat - " + MainModel.Client.Name;
            else
                Title = "Simple Chat";
        }

        #region Commands

        #region ReLoginCommand
        public RelayCommand<object> ReLoginCommand { get; private set; }
        private bool ReLoginCommandCanExecute(object arg)
        {
            return ConnectionState == ClientState.LoggedIn;
        }

        private async void ReLoginCommandExecute(object obj)
        {
            await Task.Run(() => MainModel.Client.LogoutActionRequest());
            ConnectionState = ClientState.LoggedOut;
            await Connect();

            //Application.Current.Dispatcher
            //        .BeginInvoke(new Action(()=> navigation.NavigateTo("LoginPage")));

            navigation.NavigateTo("LoginPage");
        }
        #endregion

        #region ReconnectCommand
        public RelayCommand<object> ReconnectCommand { get; private set; }

        private bool ReconnectCommandCanExecute(object arg)
        {
            return ConnectionState == ClientState.Disconnected;
        }

        private async void ReconnectCommandExecute(object obj)
        {
            await Connect();
            //if (await Connect() && MainModel.Client.Name != null)
            //{
            //    await Task.Run(() =>
            //    MainModel.Client.LoginActionRequest(MainModel.Client.Name));
            //}
        }
        #endregion

        #endregion
    }
}