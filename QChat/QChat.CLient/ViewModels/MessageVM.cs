using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.CLient.ViewModels
{
    class MessageVM
    {
        public string Sender { get; set; }
        public string Type { get; private set; }
        public object Content { get; private set; }

        public MessageVM(string sender, string type, object content)
        {
            Sender = sender;
            Type = type;
            Content = content;
        }
    }
}
