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
using ChatClient.ViewModel;

namespace ChatClient.View
{
    /// <summary>
    /// Логика взаимодействия для AuthorizePage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        AuthViewModel ViewModel { get { return DataContext as AuthViewModel; } }
        public AuthPage()
        {
            InitializeComponent();
        }

        private void BtnSignIn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DoSignIn();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Register();
        }
    }
}
