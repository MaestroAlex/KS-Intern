

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using CommonServiceLocator;
using ChatClient.Services;
using ChatClient.View;

namespace ChatClient.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            SimpleIoc.Default.Register<NetworkService>(true);
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<NavigationServiceEx>();

            Register<ShellViewModel, MainWindow>();
            Register<MainViewModel, ChatPage>();
            Register<AuthViewModel, AuthPage>();
        }

        public MainViewModel MainViewModel { get { return ServiceLocator.Current.GetInstance<MainViewModel>(); } }

        public ShellViewModel ShellViewModel {  get { return ServiceLocator.Current.GetInstance<ShellViewModel>(); } }
        public NavigationServiceEx NavigationService { get { return ServiceLocator.Current.GetInstance<NavigationServiceEx>(); } }
        public AuthViewModel AuthViewModel => ServiceLocator.Current.GetInstance<AuthViewModel>();

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