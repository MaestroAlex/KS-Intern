using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientServerLib.ClientAndServer;
using ClientServerLib.Common;
using SettingsAP;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Settings st = new Settings(ChatSyntax.SettingsFile, true);
                st.SetDefaults("ip:127.0.0.1\r\nport:15000");
                String ip = st.GetValue("ip");
                ushort port = (ushort)st.GetNumValue("port");

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
            }
            catch(Exception ex)
            { Console.WriteLine("Ошибка: " + ex.Message); }
            while (Console.ReadKey().Key != ConsoleKey.Escape) { }
        }
    }
}
