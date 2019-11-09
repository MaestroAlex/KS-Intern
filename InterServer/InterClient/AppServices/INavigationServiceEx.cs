using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterClient.AppServices
{
    public interface INavigationServiceEx
    {
        bool CanGoBack { get; }
        bool GoBack();
        bool Navigate(string pageKey, object parameter = null, NavigationTransitionInfo infoOverride = null);
        bool Navigate<TViewModel>(object parameter = null, NavigationTransitionInfo infoOverride = null);
    }
}
