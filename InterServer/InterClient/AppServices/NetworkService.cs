using NetworkLibrary;
using System.Threading.Tasks;

namespace InterClient.AppServices
{
    public class NetworkService
    {
        public delegate void MessageRecieved(string message);
        public event MessageRecieved MessageEvent;

        private string UserLogin;

        private MainClient _client;
        private bool _isLogged;

        public NetworkService()
        {
            _client = new MainClient();
            _client.MessageEvent += _client_MessageEvent;
            StartClient();
        }

        public bool IsLogged { get => _isLogged; private set => _isLogged = value; }

        private async Task<bool> StartClient()
        {
            return await _client.ConnectToServer("127.0.0.1", 47000);
        }

        public async Task SetLogin(string login)
        {
            this.UserLogin = login;
            await this.SendMessage(NetworkLibrary.Common.CommonData.LoginMessage + login);
        }

        public async Task SendMessage(string message)
        {
            await this._client.WriteToServer(message);
        }

        private void _client_MessageEvent(string message)
        {
            MessageEvent?.Invoke(message);
        }
    }
}
