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
using QChat.CLient.ViewModels;

namespace QChat.CLient.Views
{
    /// <summary>
    /// Логика взаимодействия для RegistrationView.xaml
    /// </summary>
    public partial class RegistrationView : Page
    {
        private RegistrationVM _registrationVM;

        public RegistrationView()
        {
            InitializeComponent();
            _registrationVM = DataContext as RegistrationVM;
        }

        public async void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            var passwordHash = PasswordBox.Password.GetHashCode();

            if (string.IsNullOrEmpty(LoginText.Text))
            {
                MessageBox.Show("Type in your login, pls");
                return;
            }

            if (passwordHash != PasswordVerificationBox.Password.GetHashCode())
            {
                MessageBox.Show($"Passwords don't match");
                return;
            }

            await _registrationVM.Register(LoginText.Text, passwordHash);
        }
    }
}
