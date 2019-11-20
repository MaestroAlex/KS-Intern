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



        public string Password
        {
            get { return (string)GetValue( PasswordProperty); }
            set { SetValue( PasswordProperty, value); }
        }
        public static readonly DependencyProperty  PasswordProperty =
            DependencyProperty.Register(" Password", typeof(string), typeof(AuthorizationVM), new PropertyMetadata(String.Empty));



        public AuthorizationVM()
        {
            if (!StaticProvider.IsRegistered<AuthorizationService>())
                StaticProvider.TryRegisterFactory<AuthorizationService>(() => new AuthorizationService(this));
        }

        public void Authorize()
        {
            if (_authorizationService == null) _authorizationService = StaticProvider.GetInstanceOf<AuthorizationService>();

            _authorizationService.AuthorizationInfoUpdated = false;
            //var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connection;
            //var authorizationSuccess = await _authorizationService.AuthorizeAsync(connection);
            //if (authorizationSuccess)
            //{
                StaticProvider.GetInstanceOf<NavigationService>().NavigateTo<MainView>();
            //}
        }
    }
}
