using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using QChat.CLient.Services;
using QChat.CLient.Views;

namespace QChat.CLient.ViewModels
{
    class AuthorizationVM : DependencyObject
    {
        private AuthorizationService _authorizationService;


        public string Login
        {
            get { return (string)GetValue(LoginProperty); }
            set { SetValue(LoginProperty, value); }
        }
        public static readonly DependencyProperty LoginProperty =
            DependencyProperty.Register("Login", typeof(string), typeof(AuthorizationVM), new PropertyMetadata(String.Empty));

        public void DropPassword()
        {
            StaticProvider.GetInstanceOf<AuthorizationView>().PasswordBox.Clear();
        }

        public AuthorizationVM()
        {
            StaticProvider.TryRegisterFactory<AuthorizationService>(() => new AuthorizationService(this));
            StaticProvider.TryRegisterFactory<NetworkingService>(
                () => new NetworkingService(
                    System.Net.IPAddress.Parse("127.0.0.1"), 47000
                    )
                );
        }

        public async Task Authorize(int password)
        {
            if (_authorizationService == null) _authorizationService = StaticProvider.GetInstanceOf<AuthorizationService>();

            _authorizationService.UpdateAuthorizationInfo(new Common.UserInfo { Id = Login.GetHashCode() }, password);

            var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connect();
            var authorizationSuccess = await _authorizationService.AuthorizeAsync(connection);
            if (authorizationSuccess)
            {
                StaticProvider.GetInstanceOf<NavigationService>().NavigateTo<MainView>();
            }
        }

        public void GoToRegistration()
        {
            StaticProvider.GetInstanceOf<NavigationService>().NavigateTo<RegistrationView>();
        }
    }
}
