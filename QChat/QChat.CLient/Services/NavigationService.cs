using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Windows.Controls;
using System.Windows;
using QChat.CLient.ViewModels;
using QChat.CLient.Views;
using QChat.CLient.Navigation;
using QChat.CLient;

namespace QChat.CLient.Services
{
    class NavigationService
    {
        private Dictionary<Type, object> _pages = new Dictionary<Type, object>();
        private NavigationVM _navigationVM;

        private bool _pagesLocated = false; //отвратнейший костыль

        public object StartPage { get; private set; }

        public NavigationService(NavigationVM navigationVM)
        {
            _navigationVM = navigationVM;
            LocatePages(); //BAD BOUND!!!!
            StartPage = _pages[typeof(AuthorizationView)];
        }


        public void NavigateTo<P>() where P : Page  
        {
            if (!_pages.TryGetValue(typeof(P), out var page))
                throw new NavigationFailedException();

            _navigationVM.CurrentPage = (P)page;
        }

        private void LocatePages()
        {
            if (_pagesLocated) return;

            //TODO: Separate Registration from creation

            StaticProvider.RegisterInstanceOf(new AuthorizationView());
            RegisterPage(StaticProvider.GetInstanceOf<AuthorizationView>());
            StaticProvider.RegisterInstanceOf(new MainView());
            RegisterPage(StaticProvider.GetInstanceOf<MainView>());
            StaticProvider.RegisterInstanceOf(new AuthorizationView());
            RegisterPage(StaticProvider.GetInstanceOf<AuthorizationView>());
            _pagesLocated = true;
        }

        private void RegisterPage<T>(T instance) where T : Page
        {
            _pages.Add(typeof(T), instance);
        }
    }
}
