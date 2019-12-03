using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using ChatClient.Services;

namespace ChatClient.ViewModel
{
    public class AuthViewModel : ViewModelBase
    {
        private readonly NetworkService networkService;
        private readonly NavigationServiceEx navigationService;
        private bool visibleLogin;
        public bool VisibleLogin { get { return visibleLogin; } set { Set(ref visibleLogin, value); } }
        
        public AuthViewModel(NetworkService networkService, NavigationServiceEx navigationService)
        {
            this.networkService = networkService;
            this.networkService.onSignedIn += SignedIn;
            this.navigationService = navigationService;
        }

        private void SignedIn()
        {
            VisibleLogin = true;
            navigationService.Navigate<MainViewModel>();
        }

        string login;
        public string Login
        {
            get {return login;}
            set { Set(ref login, value); }
        }
        string password;
        public string Password
        {
            get { return password; }
            set { Set(ref password, value); }
        }

        public void DoSignIn()
        {
            networkService.SignIn(Login, Password);
        }

        public void Register()
        {
            networkService.Register(Login, Password);
        }
    }
}
