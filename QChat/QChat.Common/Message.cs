using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace QChat.Common
{
    public class Message
    {
        private MessageHeader _header;
        private Content _content;

        public MessageHeader Header { get => _header; }

        public Content Content { get => _content; set => _content = value; }

        
        public Message(MessageHeader header, Content content)
        {
            _header = header;
            _content = content;
        }
    }
}
