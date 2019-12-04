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
    /// Логика взаимодействия для ChatView.xaml
    /// </summary>
    public partial class MainView : Page
    {
        private MainVM _mainVM;
        private ChatVM _chatVM;

        public MainView()
        {
            InitializeComponent();
            _mainVM = DataContext as MainVM;
            _chatVM = _mainVM.ChatVM;
        }

        private async void On_SendButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            await _chatVM.SendMessage();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var id = _chatVM.Id;
            var result = await Task.Run(() =>_mainVM.ConnectToRoom(id));

            if (result == Common.RoomConnectionResult.Success)
                _mainVM.SelectedChat.Connected = true;
        }

        private async void DisonnectButton_Click(object sender, RoutedEventArgs e)
        {
            var id = _chatVM.Id;
            await Task.Run(() => _mainVM.DisconnectFromRoom(id));

            _mainVM.SelectedChat.Connected = false;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            On_SendButton_Click(sender, e);
        }

        private void CreateRoomButton_Click(object sender, RoutedEventArgs e)
        {
            var roomDialog = new RoomDialogWindow();
            roomDialog.ShowDialog();
        }
    }
}
