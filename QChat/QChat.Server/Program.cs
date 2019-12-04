using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace QChat.Server
{
    static class Program
    {
        static void Main(string[] args)
        {
            var server = new Server();
            server.Initialize(IPAddress.Parse("127.0.0.1"), 47000);
            server.Start().Wait();
        }
    }
}
