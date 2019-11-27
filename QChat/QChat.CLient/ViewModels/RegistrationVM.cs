using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using QChat.CLient.Services;
using QChat.CLient.Views;

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
            if (await StaticProvider.GetInstanceOf<RegistrationService>().Register(login, passwordHash))
            {
                StaticProvider.GetInstanceOf<NavigationService>().NavigateTo<AuthorizationView>();
            }
            else
            {
                MessageBox.Show("RegistrationFailed");
            }
        }
    }
}
