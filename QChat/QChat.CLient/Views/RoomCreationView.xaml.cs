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
    /// Логика взаимодействия для RoomCreationView.xaml
    /// </summary>
    public partial class RoomCreationView : Page
    {
        private RoomCreationVM _roomCreationVM;

        public RoomCreationView()
        {
            InitializeComponent();
            _roomCreationVM = DataContext as RoomCreationVM;
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await _roomCreationVM.CreateRoom();

            if (result == Common.RoomCreationResult.Fail) MessageBox.Show("Failed to create room");
        }
    }
}
