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

        public IContentReciever DefineReciever(MessageHeader header) => DefineReciever(header.ContentType);
        public IContentReciever DefineReciever(ContentType type)
        {
            switch (type)
            {
                case ContentType.Text:
                    return _textReciever;
                default:
                    return null;
            }
        }

        public object ToObject(ContentType type, Content content)
        {
            switch (type)
            {
                case ContentType.Text:
                    return Encoding.Unicode.GetString(content.AsBytes());
                default:
                    return null;
            }
        }
    }
}
