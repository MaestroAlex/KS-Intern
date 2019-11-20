using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using QChat.CLient.Services;

namespace QChat.CLient.ViewModels
{
    internal class NavigationVM : DependencyObject
    {
        public object CurrentPage
        {
            get { return (object)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value);
            }
        }
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(object), typeof(NavigationVM), new PropertyMetadata());


        public NavigationVM()
        {
            if (!StaticProvider.IsRegistered<NavigationService>())
                StaticProvider.TryRegisterFactory<NavigationService>(() => new NavigationService(this));
            CurrentPage = StaticProvider.GetInstanceOf<NavigationService>().StartPage;
        }
    }
}
