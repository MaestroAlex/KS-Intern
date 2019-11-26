using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientServerLib;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            String ip = "127.0.0.1";
            ushort port = 14778;

            MainServer server = new MainServer(ip, port);
            if (server.Start().Result)
            {
                Console.WriteLine($"Запуск сервера {ip}:{port}.\nСервер начинает работу.");
            }
            else
            {
                Console.WriteLine("Ошибка при попытке запуска сервера.");
            }
            server.onServerInform += (m) => { Console.WriteLine(m); };
            while (Console.ReadKey().Key != ConsoleKey.Escape) { }
        }
    }
}
