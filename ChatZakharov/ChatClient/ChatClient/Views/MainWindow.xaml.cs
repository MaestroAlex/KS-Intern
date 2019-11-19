using ChatClient.Interface;
using ChatClient.Models;
using ChatClient.ViewModel;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatClient.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainModel.Client.Dispose();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SimpleIoc.Default.GetInstance<IFrameNavigationService>().MainFrame =
                LogicalTreeHelper.FindLogicalNode(Application.Current.MainWindow, "MainFrame") as Frame;
            SimpleIoc.Default.GetInstance<IFrameNavigationService>().NavigateTo("LoginPage");
        }
    }
}
