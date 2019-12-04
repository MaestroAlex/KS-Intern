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
using System.Windows.Shapes;
using QChat.CLient.ViewModels;
    
namespace QChat.CLient.Views
{
    /// <summary>
    /// Логика взаимодействия для RoomDialogWindow.xaml
    /// </summary>
    public partial class RoomDialogWindow : Window
    {
        private RoomDialogVM _roomPlusVM;

        public RoomDialogWindow()
        {
            InitializeComponent();

            _roomPlusVM = DataContext as RoomDialogVM;
        }

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            _roomPlusVM.GoToJoiningFrame();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            _roomPlusVM.GoToCreatingFrame();
        }
    }
}
