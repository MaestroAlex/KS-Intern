using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using QChat.CLient.Views;

namespace QChat.CLient.ViewModels
{
    class RoomDialogVM : DependencyObject
    {
        private RoomCreationView _roomCreationView;
        private JoinRoomView _joinRoomView;


        public object CurrentFrame
        {
            get { return (object)GetValue(CurrentFrameProperty); }
            set { SetValue(CurrentFrameProperty, value); }
        }
        public static readonly DependencyProperty CurrentFrameProperty =
            DependencyProperty.Register("CurrentFrame", typeof(object), typeof(RoomDialogVM), new PropertyMetadata());



        public RoomDialogVM()
        {
            _roomCreationView = new RoomCreationView();
            _joinRoomView = new JoinRoomView();

            CurrentFrame = _roomCreationView;
        }

        public void GoToJoiningFrame()
        {
            if (ReferenceEquals(CurrentFrame, _joinRoomView)) return;

            CurrentFrame = _joinRoomView;
        }

        public void GoToCreatingFrame()
        {
            if (ReferenceEquals(CurrentFrame, _roomCreationView)) return;

            CurrentFrame = _roomCreationView;
        }
    }
}
