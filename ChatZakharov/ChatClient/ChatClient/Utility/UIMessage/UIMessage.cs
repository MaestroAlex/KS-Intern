using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TransitPackage;

namespace ChatClient.Utility.UIMessage
{
    abstract class UIMessage
    {
        public Message Message { get; private set; }
        public TextBox Username { get; private set; }
        public TextBox NewMessage { get; private set; }
        public TextBox MessageTime { get; private set; }
        public Border ResultMessageBorder { get; private set; }

        protected UIMessage(Message message) => Message = message;

        protected void CreateInternalUIMessage()
        {
            Username = new TextBox()
            {
                FontSize = 12,
                Margin = new Thickness(-1, -1, -1, -4),
                Background = Brushes.Transparent,
                IsReadOnly = true,
                BorderThickness = new Thickness(0)
            };

            NewMessage = new TextBox()
            {
                Text = Message.Text,
                FontSize = 16,
                Margin = new Thickness(-1, -5, -1, -4),
                Background = Brushes.Transparent,
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true,
                BorderThickness = new Thickness(0)
            };

            MessageTime = new TextBox()
            {
                Text = DateTime.Now.ToString("MM.dd.yyyy HH:mm"),
                HorizontalAlignment = HorizontalAlignment.Right,
                FontSize = 10,
                Margin = new Thickness(-1, -1, 2, -1),
                Background = Brushes.Transparent,
                IsReadOnly = true,
                BorderThickness = new Thickness(0)
            };

            StackPanel resultMessage = new StackPanel();
            resultMessage.Children.Add(Username);
            resultMessage.Children.Add(NewMessage);
            resultMessage.Children.Add(MessageTime);

            ResultMessageBorder = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(0, 10, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Child = resultMessage
            };
        }
        public abstract void CreateUIMessage();
    }
}
