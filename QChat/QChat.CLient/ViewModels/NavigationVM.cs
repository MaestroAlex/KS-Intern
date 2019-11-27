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
        public event EventHandler PageChanging;
        public event EventHandler PageChanged;

        public object CurrentPage
        {
            get { return (object)GetValue(CurrentPageProperty); }
            set {
                PageChanging?.Invoke(this, null);
                SetValue(CurrentPageProperty, value);
                PageChanged?.Invoke(this, null);
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
