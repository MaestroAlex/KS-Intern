using NetworkLibrary.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLibrary
{

    public class MainServer : NetworkCore
    {
		private TcpListener _tcpListener;

		private List<ClientEssence> _clients = new List<ClientEssence>();

		private bool _shouldWork;

		public MainServer(string ip, ushort port)
		{
			_tcpListener = new TcpListener(IPAddress.Parse(ip), port);

            this.MessageEvent += this.InterceptMessage;
		}

        public async Task<bool> StartServer()
		{
			bool res = false;
			try
			{
				_tcpListener.Start();
				StartAcceptClients();
				res = true;
			}
			catch(Exception e)
			{
				Debug.WriteLine(e);
			}
			return res;
		}

        private async Task ListenToClient(ClientEssence cl)
        {
            await base.ListenToClient(cl);
            _clients.Remove(cl);
        }

        private async void StartAcceptClients()
		{
			_shouldWork = true;
			while(_shouldWork)
			{
				var client = new ClientEssence(await _tcpListener.AcceptTcpClientAsync());
                this._clients.Add(client);
				this.ListenToClient(client);
			}
			_clients.Clear();
			_tcpListener.Stop();
		}

        private void InterceptMessage(string message, object sender)
        {
            var client = sender as ClientEssence;
            if(client.UserId == 0 && !message.StartsWith(CommonData.LoginMessage))
            {
                Console.WriteLine("Unregistered user message : " + message);
                return;
            }
            if(message.StartsWith(CommonData.LoginMessage))
            {
                var login = message.Substring(2);
                if(string.IsNullOrEmpty(login.Trim()))
                {
                    Console.WriteLine("Uncorrect login message");
                    return;
                }
                else
                {
                    client.DoLogin(login);
                    WriteToAllClients($"User {login} joined chat!");
                }
            }
            else
            {
                WriteToAllClients($"Message from {client.UserLogin}: " + message);
            }
        }

        private async Task WriteToAllClients(string message)
		{
			foreach(var cl in _clients)
			{
				WriteToClient(cl, message);
			}
		}
    }
}
