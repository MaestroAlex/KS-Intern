using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using InterClient.AppServices;

namespace InterClient.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly NetworkService networkService;
        private readonly NavigationServiceEx navigationService;
        private bool _isLogged = false;
        public MainViewModel(NetworkService networkService, NavigationServiceEx navigationService)
        {
            this.networkService = networkService;
            this.navigationService = navigationService;

            this.networkService.MessageEvent += NetworkService_MessageEvent;
        }

        public async void SendButtonClick()
        {
            await this.networkService.SendMessage(this.UserMessage);
            this.UserMessage = null;
        }

        internal void OnWindowsLoaded()
        {
            if(this.networkService.IsLogged == false)
            {
                this.navigationService.Navigate<MainViewModel>();
            }
        }

        private void NetworkService_MessageEvent(string message)
        {
            this.MainText += "\n" + message;
        }

        private async void RequestUserName()
        {
        }
        

        private string mainText;
        public string MainText
        {
            get => this.mainText;
            set => this.Set(ref this.mainText, value);
        }
        private string userMessage;
        public string UserMessage
        {
            get => this.userMessage;
            set => this.Set(ref this.userMessage, value);
        }
    }
}