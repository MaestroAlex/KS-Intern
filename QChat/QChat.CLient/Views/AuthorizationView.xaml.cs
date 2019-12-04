﻿using System;
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
    /// Логика взаимодействия для AuthorizationView.xaml
    /// </summary>
    public partial class AuthorizationView : Page
    {
        private AuthorizationVM _authorizationVM;

        public AuthorizationView()
        {
            InitializeComponent();
            _authorizationVM = StaticProvider.GetInstanceOf<AuthorizationVM>();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await _authorizationVM.Authorize(PasswordBox.Password.GetHashCode());
        }

        private void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            _authorizationVM.GoToRegistration();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
    }
}
