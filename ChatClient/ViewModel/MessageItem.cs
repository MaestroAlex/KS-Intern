using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace ChatClient.ViewModel
{
    public class MessageItem
    {
        public string UserName { get; set; }
        public string Message { get; set; }
        public BitmapImage Image {get; set;}

        public MessageItem(string UserName, string Message)
        {
            this.UserName = UserName;
            this.Message = Message;
        }

        public MessageItem(string UserName, BitmapImage Image)
        {
            this.UserName = UserName;
            this.Image = Image;
        }
    }
}
