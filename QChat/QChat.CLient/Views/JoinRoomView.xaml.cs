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
    /// Логика взаимодействия для JoinRoomView.xaml
    /// </summary>
    public partial class JoinRoomView : Page
    {
        private RoomJoiningVM _joiningVM;

        public JoinRoomView()
        {
            InitializeComponent();

            _joiningVM = DataContext as RoomJoiningVM;
        }

        public async void JoinButton_Click(object sender, RoutedEventArgs args)
        {
            await _joiningVM.JoinRoom();
        }
    }
}
