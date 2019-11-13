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
    class UIMessageOutput : UIMessage
    {
        public UIMessageOutput(Message message) : base(message) { }

        public override void CreateUIMessage()
        {
            CreateInternalUIMessage();
            Username.Text = Message.From + ":";
            Username.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF1BA1E2");
            MessageTime.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF1BA1E2");
            ResultMessageBorder.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF1BA1E2");
            ResultMessageBorder.HorizontalAlignment = HorizontalAlignment.Right;
        }
    }
}
