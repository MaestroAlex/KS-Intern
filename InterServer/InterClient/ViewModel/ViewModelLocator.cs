/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:InterClient"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using InterClient.AppServices;
using InterClient.Views;
using System.Windows.Controls;

namespace InterClient.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            SimpleIoc.Default.Register<NetworkService>(true);
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register(() => new NavigationServiceEx());

            this.Register<ShellViewModel, MainWindow>();
            this.Register<MainViewModel, ChatPage>();
            this.Register<AuthViewModel, AuthPage>();
        }

        public MainViewModel MainViewModel => ServiceLocator.Current.GetInstance<MainViewModel>();
        public NavigationServiceEx NavigationService => ServiceLocator.Current.GetInstance<NavigationServiceEx>();
        public AuthViewModel AuthViewModel => ServiceLocator.Current.GetInstance<AuthViewModel>();
        public ShellViewModel ShellViewModel => ServiceLocator.Current.GetInstance<ShellViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }

        public void Register<VM, V>()
            where VM : class
            where V : class
        {
            SimpleIoc.Default.Register<VM>();
            SimpleIoc.Default.Register<V>();

            this.NavigationService.Configure(typeof(VM).FullName, typeof(V));
        }
    }
}