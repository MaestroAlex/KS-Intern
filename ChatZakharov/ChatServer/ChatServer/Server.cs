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

        public int ConnectedIpConnectionCheckTimerInterval { get; private set; }

        TcpListener listener;
        Dictionary<string, List<Client>> roomUsersDictionary;
        List<Client> connectedIp;
        Mutex connectedIpChanged;
        System.Timers.Timer connectedIpConnectionCheckTimer;

        public Server(Config config)
        {
            listener = new TcpListener(config.Ip, config.Port);
            listener.Server.ReceiveTimeout = config.ConnectionTimeoutMs;
            listener.Server.SendTimeout = config.ConnectionTimeoutMs;
            DB.Initialize(config.ConnectionString);
            if (config.FirstRun)
            {
                if (DB.CreateDB().Result)
                {
                    config.SetConnectionString();
                    Console.WriteLine("Database successfully created");
                }
                else
                {
                    Console.WriteLine("Exception in database creation");
                    return;
                }
            }
            ConnectedIpConnectionCheckTimerInterval = Config.GetConfig().ConnectionTimeoutMs + 2000;
            roomUsersDictionary = new Dictionary<string, List<Client>>();
            connectedIp = new List<Client>();
            connectedIpChanged = new Mutex();
            connectedIpConnectionCheckTimer = new System.Timers.Timer();
            connectedIpConnectionCheckTimer.Elapsed += ConnectedIpConnectionCheck;
            connectedIpConnectionCheckTimer.Interval = ConnectedIpConnectionCheckTimerInterval;
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

        private async Task ProcessClient(TcpClient tcpClient)
        {
            Client client = new Client(tcpClient, this);

            connectedIpChanged.WaitOne();
            connectedIp.Add(client);
            connectedIpChanged.ReleaseMutex();

            await Task.Run(() => client.DefineAction());
        }

        //TODO заменить везде фурычи на линкью, убрать мьютексы
        private void ConnectedIpConnectionCheck(object sender, EventArgs e)
        {
            connectedIpChanged.WaitOne();

            for (int i = connectedIp.Count - 1; i >= 0; i--)
                if (!connectedIp[i].IsBusy)
                    connectedIp[i].ActionQueue.ConnectionCheckRequired = true;

            connectedIpChanged.ReleaseMutex();
        }


        public bool UserExist(string name)
        {
            connectedIpChanged.WaitOne();

            bool res = connectedIp.Exists(client => client.Name == name);

            connectedIpChanged.ReleaseMutex();
            return res;
        }

        public void ChannelCreatedNotify(Channel channel, string channelCreator)
        {
            connectedIpChanged.WaitOne();

            foreach (var item in connectedIp)
                if (!string.IsNullOrWhiteSpace(item.Name) &&
                    item.Name != channel.Name &&
                    item.Name != channelCreator)
                    item.ActionQueue.NewChannels.Enqueue(channel);

            connectedIpChanged.ReleaseMutex();
        }

        public void ChannelDeletedNotify(Channel channel)
        {
            connectedIpChanged.WaitOne();

            foreach (var item in connectedIp)
                if (item.Name != channel.Name)
                    item.ActionQueue.DeletedChannels.Enqueue(channel);

            connectedIpChanged.ReleaseMutex();
        }

        public void SendToOnlineClientsRoom(Message message)
        {
            connectedIpChanged.WaitOne();

            List<Client> clientsToSend = roomUsersDictionary
                .Where(node => node.Key == message.To)
                .First().Value;

            foreach (var item in clientsToSend)
                if (item.Name != message.From)
                    item.ActionQueue.Messages.Enqueue(message);

            connectedIpChanged.ReleaseMutex();
        }

        public bool SendToOnliceClient(Message message)
        {
            connectedIpChanged.WaitOne();

            connectedIp.Find(client => client.Name == message.To)
                ?.ActionQueue
                .Messages
                .Enqueue(message);

            connectedIpChanged.ReleaseMutex();
            return true;

            //todo как реализовать отчет о доставке сообщения
        }

        public void RemoveFromConnectedIp(Client client)
        {
            connectedIpChanged.WaitOne();
            connectedIp.Remove(client);
            connectedIpChanged.ReleaseMutex();
        }

        public void AddedOnlineRoomsUser(Client client, List<Channel> channels)
        {
            foreach (var item in channels)
            {
                if ((item.Type == ChannelType.public_open ||
                    item.Type == ChannelType.public_closed) && item.IsEntered == true)
                {
                    if (!roomUsersDictionary.ContainsKey(item.Name))
                        roomUsersDictionary.Add(item.Name, new List<Client>() { client });
                    else
                        roomUsersDictionary.Where((node) => node.Key == item.Name).First().Value.Add(client);
                }
            }
        }

        public void AddedOnlineRoomUser(Client client, string channelName)
        {
            if (!roomUsersDictionary.ContainsKey(channelName))
                roomUsersDictionary.Add(channelName, new List<Client>() { client });
            else
                roomUsersDictionary.Where(node => node.Key == channelName).First().Value.Add(client);
        }

        public void RemoveOnlineRoomsUser(Client client)
        {
            foreach (var room in roomUsersDictionary)
                if (room.Value.Contains(client))
                    room.Value.Remove(client);
        }

        public void RemoveOnlineRoomUser(Client client, string roomName)
        {
            roomUsersDictionary
                .Where(node => node.Key == roomName)
                .First().Value.Remove(client);
        } 
    }
}
