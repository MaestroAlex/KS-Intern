using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatClient.Services;
using System.Windows.Controls;

namespace ChatClient.ViewModel
{
    public class ShellViewModel
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
