using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Common.Net
{
    public class ContentRecieverTable
    {
        private TextReciever _textReciever;


        public ContentRecieverTable()
        {
            _textReciever = new TextReciever();
        }

        public T Get<T>() where T : TextReciever
        {
            return (T)_textReciever;
        }        
    }
}
