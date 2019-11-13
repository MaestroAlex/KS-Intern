using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(Config.GetConfig());
            Task.Run(() => server.Start()).Wait();
        }
    }
}
