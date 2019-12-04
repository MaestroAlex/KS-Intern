using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using QChat.CLient.Services;
using QChat.CLient.Views;
using QChat.Common;

namespace QChat.CLient.ViewModels
{
    class RegistrationVM : DependencyObject
    {
        public string Login
        {
            get { return (string)GetValue(LoginProperty); }
            set { SetValue(LoginProperty, value); }
        }

        public static readonly DependencyProperty LoginProperty =
            DependencyProperty.Register("Login", typeof(string), typeof(RegistrationVM), new PropertyMetadata(string.Empty));



        public RegistrationVM()
        {
            StaticProvider.RegisterFactory<RegistrationService>(() => new RegistrationService());
        }

        public async Task Register(string login, int passwordHash)
        {
            switch (await StaticProvider.GetInstanceOf<RegistrationService>().Register(login, passwordHash))
            {
                case RegistrationResult.Success:
                    StaticProvider.GetInstanceOf<NavigationService>().NavigateTo<AuthorizationView>();
                    break;
                case RegistrationResult.NicknameAlreadyRegistered:
                    MessageBox.Show("Nickname already registered.");
                    break;
                case RegistrationResult.Fail:
                    MessageBox.Show("Registration failed");
                    break;
            }
        }
    }
}
