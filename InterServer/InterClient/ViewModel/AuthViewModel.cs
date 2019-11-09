using GalaSoft.MvvmLight;
using InterClient.AppServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        private string login;
        public string UserLogin
        {
            get => this.login;
            set => this.Set(ref this.login, value);
        }

        public async Task<bool> Auth()
        {
            bool res = false;
            await this.networkService.SetLogin(this.UserLogin);
            this.navigationService.Navigate<MainViewModel>();
            return res;
        }
    }
}
