using NetworkLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterServer
{
	class Program
	{
		static void Main(string[] args)
		{
			    ServerTask("25.70.5.77", 47000).Wait();
			Console.ReadKey();
		}

		static async Task ServerTask(string ip, ushort port)
		{
			Console.WriteLine($"Starting server on {ip}:{port} ...");
			MainServer _server = new MainServer(ip, port);
			var started = await _server.StartServer();
			if(started)
			{
				Console.WriteLine("Server started");
			}
			else
			{
				Console.WriteLine("Server failed to start");
				return;
			}
			await Task.Delay(15000000);
		}
	}
}
