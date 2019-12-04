using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using QChat.CLient.Services;
using QChat.Common;

namespace QChat.CLient.ViewModels
{
    class RoomCreationVM : DependencyObject
    {
        public string RoomName
        {
            get { return (string)GetValue(RoomNameProperty); }
            set { SetValue(RoomNameProperty, value); }
        }
        public static readonly DependencyProperty RoomNameProperty =
            DependencyProperty.Register("RoomName", typeof(string), typeof(RoomCreationVM), new PropertyMetadata(string.Empty));


        public async Task<RoomCreationResult> CreateRoom()
        {
            var roomingService = StaticProvider.GetInstanceOf<RoomingService>();
            var name = RoomName;

            var result = Task.Run(() => roomingService.CreateRoom(name)).Result;

            if (result == RoomCreationResult.Success)
            {
                
            }

            return result;
        }
    }
}
