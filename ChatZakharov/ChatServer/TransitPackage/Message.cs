using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitPackage
{
    [Serializable]
    public class Message
    {
        public string From { get; set; }
        public string To { get; set; }
        public string ChatDestination { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
    }
}
