using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using InterClient.AppServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InterClient.ViewModel
{
    public class AuthViewModel : ViewModelBase
    {
        private readonly NetworkService networkService;
        private readonly NavigationServiceEx navigationService;
        public AuthViewModel(NetworkService networkService, NavigationServiceEx navigationService)
        {
            this.networkService = networkService;
            this.navigationService = navigationService;
            Messenger.Default.Register<AppMessage>(this, this.OnLoginComleted);
        }

        private void OnLoginComleted(AppMessage obj)
        { 
            if(obj == AppMessage.LoginSequenceComplited)
            {
                this.navigationService.Navigate<MainViewModel>();
            }
            else if(obj == AppMessage.LoginFailed)
            {
                MessageBox.Show("Login credentials are invalid", "Error");
            }
        }

        private string login;
        public string UserLogin
        {
            get => this.login;
            set => this.Set(ref this.login, value);
        }

        public async Task<bool> Auth(string password)
        {
            bool res = false;
            await this.networkService.SetLogin(this.UserLogin, password);
            return res;
        }
    }
}
