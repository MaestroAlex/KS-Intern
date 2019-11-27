using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.CLient.Views
{
    class ViewLocator
    {
        public ViewLocator()
        {
            StaticProvider.TryRegisterFactory<MainView>(() => new MainView());
            StaticProvider.TryRegisterFactory<AuthorizationView>(() => new AuthorizationView());
            StaticProvider.TryRegisterFactory<RegistrationView>(() => new RegistrationView());
        }

        public MainView MainView => StaticProvider.GetInstanceOf<MainView>();
        public AuthorizationView AuthorizationView => StaticProvider.GetInstanceOf<AuthorizationView>();
        public RegistrationView RegistrationView => StaticProvider.GetInstanceOf<RegistrationView>();
    }
}
