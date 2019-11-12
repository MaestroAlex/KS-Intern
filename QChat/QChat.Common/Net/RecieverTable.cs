using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Common.Net
{
    public class RecieverTable
    {
        private TextReciever _textReciever;

        public T Get<T>() where T : TextReciever
        {
            return (T)_textReciever;
        }
    }
}
