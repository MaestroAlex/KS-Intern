/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:ChatClient"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using System;
using ChatClient.Interface;
using ChatClient.ViewModel.ChatViewModel;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;

namespace ChatClient.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        /// 
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            InitializeNavigation();
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<LoginViewModel>();
            SimpleIoc.Default.Register<ChatsViewModel>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public LoginViewModel Login => ServiceLocator.Current.GetInstance<LoginViewModel>();
        public ChatsViewModel Chats => ServiceLocator.Current.GetInstance<ChatsViewModel>();

        public void InitializeNavigation()
        {
            ViewModelNavigation navigation = new ViewModelNavigation();
            navigation.Register("LoginPage", new Uri("../Views/LoginPage.xaml", UriKind.Relative));
            navigation.Register("ChatsPage", new Uri("../Views/ChatView/ChatsPage.xaml", UriKind.Relative));
            navigation.Register("NoUsersPage", new Uri("../Views/ChatView/NoUsersChatPage.xaml", UriKind.Relative));
            SimpleIoc.Default.Register<IFrameNavigationService>(() => navigation);
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}


