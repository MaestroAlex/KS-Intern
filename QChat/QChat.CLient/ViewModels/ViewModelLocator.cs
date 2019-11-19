using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.CLient.ViewModels
{
    class ViewModelLocator
    {
        public ViewModelLocator()
        {
            StaticProvider.RegisterInstanceOf(new AuthorizationVM());
            StaticProvider.RegisterInstanceOf(new MainVM());

            //carefull - bad bound to alocated viewmodels
            StaticProvider.RegisterInstanceOf(new NavigationVM());
        }

        public NavigationVM NavigationVM => StaticProvider.GetInstanceOf<NavigationVM>();
        public AuthorizationVM AuthorizationVM => StaticProvider.GetInstanceOf<AuthorizationVM>();
        public MainVM MainVM => StaticProvider.GetInstanceOf<MainVM>();
    }
}
