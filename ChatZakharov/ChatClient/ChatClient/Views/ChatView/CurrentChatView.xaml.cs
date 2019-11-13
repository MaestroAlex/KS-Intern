using ChatClient.ViewModel.ChatViewModel;
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
    /// Логика взаимодействия для CurrentChatView.xaml
    /// </summary>
    public partial class CurrentChatView : Grid
    {
        public CurrentChatView()
        {
            InitializeComponent();
        }

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CurrentChatViewModel cur = DataContext as CurrentChatViewModel;
            cur.CurrentMessage = curMessage;
            cur.MessagesHistoryGrid = HistoryGrid;
            curMessage.Focus();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            curMessage.Focus();
        }
    }
}
