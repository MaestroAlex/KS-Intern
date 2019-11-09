using GalaSoft.MvvmLight;
using InterClient.AppServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace InterClient.ViewModel
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly NavigationServiceEx navigationService;
        public ShellViewModel(NavigationServiceEx navigationService)
        {
            this.navigationService = navigationService;
        }

        public void OnPageLoaded(Frame frame)
        {
            this.navigationService.Frame = frame;

            this.navigationService.Navigate<AuthViewModel>();
        }
    }
}
