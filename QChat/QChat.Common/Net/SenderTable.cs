using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Common.Net
{
    public class SenderTable
    {
        private TextSender _textSender;

        public T GetSender<T>() where T : TextSender
        {
            return (T)_textSender;
        }
    }
}
