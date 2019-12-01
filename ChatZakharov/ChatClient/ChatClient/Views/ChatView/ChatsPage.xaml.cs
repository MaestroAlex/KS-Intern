using ChatClient.ViewModel.ChatViewModel;
using MahApps.Metro.Controls;
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

namespace ChatClient.Views.ChatView
{
    /// <summary>
    /// Логика взаимодействия для ChatsPageView.xaml
    /// </summary>
    public partial class ChatsPage : Page
    {
        public ChatsPage()
        {
            InitializeComponent();
            (DataContext as ChatsViewModel).ViewChannelsControl = HamburgerMenuControl;
        }

        private void HamburgerMenu_ItemClick(object sender, ItemClickEventArgs e)
        {
            HamburgerMenuControl.Content = e.ClickedItem;
        }
    }
}
