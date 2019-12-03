using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace ChatClient.ViewModel
{
    public class MessageItem
    {
        public string UserName { get; set; }
        public string Message { get; set; }

        public MessageItem(string UserName, string Message)
        {
            this.UserName = UserName + (UserName == string.Empty ? "" : ":");
            this.Message = Message;
        }
    }
}
