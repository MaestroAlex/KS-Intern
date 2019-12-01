using ChatMVVM.ViewModels;
using System.Windows.Controls;

namespace ChatMVVM.Views
{
    /// <summary>
    /// Interaction logic for ChatPage.xaml
    /// </summary>
    /// 

    public partial class ChatPage : Page
    {
        public ChatPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            (DataContext as ChatViewModel).ChatRoomButton(sender);
        }
    }   
}
