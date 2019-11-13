using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using TransitPackage;

namespace ChatServer
{
    class Server
    {
        TcpListener listener;
        List<Client> connectedIp;
        Mutex connectedIpChanged;
        System.Timers.Timer connectedIpConnectionCheckTimer;

        public Server(Config config)
        {
            listener = new TcpListener(config.Ip, config.Port);
            listener.Server.ReceiveTimeout = config.ReceiveTimeoutMs;
            listener.Server.SendTimeout = config.SendTimeoutMs;

            connectedIp = new List<Client>();
            connectedIpChanged = new Mutex();
            connectedIpConnectionCheckTimer = new System.Timers.Timer();
            connectedIpConnectionCheckTimer.Elapsed += ConnectedIpConnectionCheck;
            connectedIpConnectionCheckTimer.Interval = 5000;
        }

        public async Task Start()
        {
            listener.Start();
            connectedIpConnectionCheckTimer.Start();
            Console.WriteLine("Server is up");
            while (true)
            {
                TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                ProcessClient(tcpClient);
            }
        }

        //TODO заменить везде фурычи на линкью, убрать мьютексы
        private void ConnectedIpConnectionCheck(object sender, EventArgs e)
        {
            connectedIpChanged.WaitOne();

            for (int i = connectedIp.Count - 1; i >= 0; i--)
                if (!connectedIp[i].ActionQueue.IsBusy)
                    connectedIp[i].ActionQueue.ConnectionCheckRequired = true;

            connectedIpChanged.ReleaseMutex();
        }

        private async Task ProcessClient(TcpClient tcpClient)
        {
            Client client = new Client(tcpClient, this);

            connectedIpChanged.WaitOne();
            connectedIp.Add(client);
            connectedIpChanged.ReleaseMutex();

            await Task.Run(() => client.DefineAction());
        }


        public bool UserExist(string name)
        {
            connectedIpChanged.WaitOne();

            bool res = connectedIp.Exists(client => client.Name == name);

            connectedIpChanged.ReleaseMutex();
            return res;
        }

        public void UserLoggedInNotify(string name)
        {
            connectedIpChanged.WaitOne();

            foreach (var item in connectedIp)
                if (!string.IsNullOrWhiteSpace(item.Name) && item.Name != name)
                    item.ActionQueue.NewUsers.Enqueue(name);

            connectedIpChanged.ReleaseMutex();
        }

        public void UserLoggedOutNotify(string name)
        {
            connectedIpChanged.WaitOne();

            foreach (var item in connectedIp)
                if (item.Name != name)
                    item.ActionQueue.DeletedUsers.Enqueue(name);

            connectedIpChanged.ReleaseMutex();
        }

        public bool SendToclient(Message message)
        {
            connectedIpChanged.WaitOne();

            connectedIp.Find(client => client.Name == message.To)
                .ActionQueue
                .Messages
                .Enqueue(message);

            connectedIpChanged.ReleaseMutex();
            return true;

            //todo как реализовать отчет о доставке сообщения
        }

        public List<string> GetUserList(string forUser)
        {
            connectedIpChanged.WaitOne();

            List<string> res = connectedIp
                    .Select(elem => elem.Name)
                    .Where(name => !string.IsNullOrWhiteSpace(name) && name != forUser)
                    .ToList();

            connectedIpChanged.ReleaseMutex();
            return res;
        }

        public void RemoveFromConnectedIp(Client client)
        {
            connectedIpChanged.WaitOne();
            connectedIp.Remove(client);
            connectedIpChanged.ReleaseMutex();
        }
    }
}
