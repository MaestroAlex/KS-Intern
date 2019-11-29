using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitPackage
{

    public enum MessageType { text = 1, image = 2, document = 3 }
    [Serializable]
    public class Message
    {
        public string From { get; set; }
        public string To { get; set; }
        public string ChatDestination { get; set; }
        public MessageType MessageType { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
    }
}
