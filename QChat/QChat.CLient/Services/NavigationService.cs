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
        private NavigationVM _navigationVM;
        private ViewLocator _viewLocator = new ViewLocator();
        private Dictionary<Type, Func<object>> _pagesProperties = new Dictionary<Type, Func<object>>();

        public object StartPage { get; private set; }

        public NavigationService(NavigationVM navigationVM)
        {
            _navigationVM = navigationVM;

            RegisterPage(() => _viewLocator.AuthorizationView);
            RegisterPage(() => _viewLocator.MainView);

            StartPage = _viewLocator.AuthorizationView;
        }


        public void NavigateTo<P>() where P : Page  
        {
            if (!_pagesProperties.TryGetValue(typeof(P), out var pageProperty))
                throw new NavigationFailedException();

            _navigationVM.CurrentPage = (P)pageProperty();
        }

        public void RegisterPage<T>(Func<T> property) where T : Page
        {
            _pagesProperties.Add(typeof(T), property);
        }
    }
}
