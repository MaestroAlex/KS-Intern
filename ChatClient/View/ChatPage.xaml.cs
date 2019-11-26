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
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class ChatPage : Page
    {
        MainViewModel ViewModel { get { return DataContext as MainViewModel; } }
        public ChatPage()
        {
            InitializeComponent();
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SendMessage();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BtnSend_Click(null, null);
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GetHelpMessage();
        }

        private void Room_Selected(object sender, RoutedEventArgs e)
        {
            ViewModel.ChatRoomSelected(sender);
        }
    }
}
