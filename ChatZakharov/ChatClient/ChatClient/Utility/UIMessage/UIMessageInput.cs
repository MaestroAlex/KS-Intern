using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TransitPackage;

namespace ChatClient.Utility.UIMessage
{
    class UIMessageInput : UIMessage
    {
        public UIMessageInput(Message message) : base(message) { }

        public override void CreateUIMessage()
        {
            CreateInternalUIMessage();
            Username.Text = Message.From + ":";
            Username.Foreground = Brushes.NavajoWhite;
            MessageTime.Foreground = Brushes.NavajoWhite;
            ResultMessageBorder.BorderBrush = Brushes.NavajoWhite;
            ResultMessageBorder.HorizontalAlignment = HorizontalAlignment.Left;
        }
    }
}
