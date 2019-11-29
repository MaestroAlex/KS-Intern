using ChatClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TransitPackage;

namespace ChatClient.Utility.UIMessage
{
    abstract class UIMessage
    {
        public Message Message { get; private set; }
        public TextBox Username { get; private set; }
        public FrameworkElement Content { get; private set; }
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

            Content = DefineContent();

            MessageTime = new TextBox()
            {
                Text = Message.Date.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
                HorizontalAlignment = HorizontalAlignment.Right,
                FontSize = 10,
                Margin = new Thickness(-1, -1, 2, -1),
                Background = Brushes.Transparent,
                IsReadOnly = true,
                BorderThickness = new Thickness(0)
            };

            StackPanel resultMessage = new StackPanel();
            resultMessage.Children.Add(Username);
            resultMessage.Children.Add(Content);
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

        private FrameworkElement DefineContent()
        {
            if (Message.MessageType == MessageType.text)
            {
                TextBox res = new TextBox()
                {
                    Text = Message.Content,
                    FontSize = 16,
                    Margin = new Thickness(-1, -5, -1, -4),
                    Background = Brushes.Transparent,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true,
                    BorderThickness = new Thickness(0),
                };
                return res;
            }
            else if (Message.MessageType == MessageType.image)
            {
                byte[] imageArr = Convert.FromBase64String(Message.Content);
                Image res = new Image()
                {
                    Source = ByteImageConverter.ByteToImage(imageArr),
                    Margin = new Thickness(5),
                    MaxHeight = 500
                };
                return res;
            }
            else if (Message.MessageType == MessageType.document)
            {
                TextBox res = new TextBox()
                {
                    Text = "sorry, " + MainModel.Client.Name + " tries to send document or the type of pic is invalid, have a nice day",
                    FontSize = 16,
                    Margin = new Thickness(-1, -5, -1, -4),
                    Background = Brushes.Transparent,
                    Foreground = Brushes.Red,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true,
                    BorderThickness = new Thickness(0),
                };
                return res;
            }

            return null;
        }

        public abstract void CreateUIMessage();
    }
}
