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
    class RoomJoiningVM : DependencyObject
    {
        public string RoomId
        {
            get { return (string)GetValue(RoomIdProperty); }
            set { SetValue(RoomIdProperty, value); }
        }
        public static readonly DependencyProperty RoomIdProperty =
            DependencyProperty.Register("RoomId", typeof(string), typeof(RoomJoiningVM), new PropertyMetadata(string.Empty));


        public async Task<RoomJoiningResult> JoinRoom()
        {
            var roomingService = StaticProvider.GetInstanceOf<RoomingService>();

            if (!int.TryParse(RoomId, out var id))
            {
                MessageBox.Show("Incorect room id. Use digitals only");
                return RoomJoiningResult.Fail;
            }

            var result = Task.Run(() => roomingService.JoinRoom(id)).Result;

            switch (result)
            {
                case Common.RoomJoiningResult.Fail:
                    MessageBox.Show("Failed to join room");
                    break;
                case Common.RoomJoiningResult.RoomIsNotFound:
                    MessageBox.Show("Room was not found");
                    break;
                case Common.RoomJoiningResult.RoomIsNotPublic:
                    MessageBox.Show("This room can be joined only by invitation");
                    break;
                case RoomJoiningResult.Success:
                    MessageBox.Show("Joined room");
                    break;
                case RoomJoiningResult.AlreadyMember:
                    MessageBox.Show("Aready member of this room");
                    break;
            }

            return result;
        }
    }
}
