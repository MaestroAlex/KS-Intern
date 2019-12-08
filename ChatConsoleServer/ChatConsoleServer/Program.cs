namespace ChatConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server("192.168.1.108", 904);
            server.StartServer().Wait();
        }
    }
}
