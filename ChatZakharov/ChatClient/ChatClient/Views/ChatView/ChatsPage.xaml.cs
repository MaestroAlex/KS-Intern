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
        private static bool userCollectionSigned = false;
        private HamburgerMenuItemCollection channels;
        public ChatsPage()
        {
            InitializeComponent();
            if (!userCollectionSigned)
            {
                channels = (DataContext as ChatsViewModel).Channels;
                channels.Changed += Users_Changed;
                userCollectionSigned = true;
            }
        }

        private void HamburgerMenu_ItemClick(object sender, ItemClickEventArgs e)
        {
            HamburgerMenuControl.Content = e.ClickedItem;
        }

        private void Users_Changed(object sender, EventArgs e)
        {
            if (channels.Count == 1)
                HamburgerMenuControl.Content = channels.First();
        }
    }
}
