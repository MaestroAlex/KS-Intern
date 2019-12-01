using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransitPackage;

namespace ChatServer
{
    class Client
    {
        public TcpClient TcpClient { get; private set; }
        public DataQueue ActionQueue { get; private set; }
        public string Name { get; private set; }
        public bool IsBusy { get; private set; }

        Server server;
        NetworkStream networkStream;
        NetworkMessageStream messageStream;
        bool streamIsOpen;

        public Client(TcpClient client, Server server)
        {
            this.server = server;
            this.TcpClient = client;
            networkStream = client.GetStream();
            messageStream = new NetworkMessageStream(networkStream);
            ActionQueue = new DataQueue();
            streamIsOpen = true;
            Console.WriteLine("User {0} connected ",
                ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
        }

        public async Task DefineAction()
        {
            try
            {
                while (streamIsOpen)
                {
                    if (networkStream.DataAvailable)
                    {
                        IsBusy = true;
                        NetworkMessage request = await messageStream.ReadAsync();
                        await Process(request);
                        IsBusy = false;
                    }
                    else if (ActionQueue.Messages.Count != 0)
                    {
                        IsBusy = true;
                        NetworkMessage response = new NetworkMessage(ActionEnum.send_message);
                        await Process(response);
                        IsBusy = false;
                    }
                    else if (ActionQueue.NewChannels.Count != 0)
                    {
                        IsBusy = true;
                        NetworkMessage response = new NetworkMessage(ActionEnum.channel_created);
                        await Process(response);
                        IsBusy = false;
                    }
                    else if (ActionQueue.DeletedChannels.Count != 0)
                    {
                        IsBusy = true;
                        NetworkMessage response = new NetworkMessage(ActionEnum.channel_deleted);
                        await Process(response);
                        IsBusy = false;
                    }
                    else if (ActionQueue.ConnectionCheckRequired)
                    {
                        IsBusy = true;
                        NetworkMessage response = new NetworkMessage(ActionEnum.connection_check);
                        await Process(response);
                        IsBusy = false;
                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Something gone wrong in DefineAction !!!");
                Console.WriteLine(e.Message);
                await LogoutActionResponse();
            }
        }

        private async Task Process(NetworkMessage message)
        {
            try
            {
                switch (message.Action)
                {
                    case ActionEnum.aes_handshake:
                        await AESHandshakeWithRSAActionResponse();
                        break;
                    case ActionEnum.login:
                        await LoginActionResponse(message);
                        break;
                    case ActionEnum.logout:
                        await LogoutActionResponse();
                        break;
                    case ActionEnum.send_message:
                        await SendMessageActionResponse(message);
                        break;
                    case ActionEnum.receive_message:
                        await ReceiveMessageActionResponse(message);
                        break;
                    case ActionEnum.get_channels:
                        await GetChannelsActionResponse();
                        break;
                    case ActionEnum.get_all_history:
                        await GetAllHistoryActionResponse();
                        break;
                    case ActionEnum.get_room_history:
                        await GetRoomHistoryActionResponse(message);
                        break;
                    case ActionEnum.create_user:
                        await CreateUserActionResponse(message);
                        break;
                    case ActionEnum.create_room:
                        await CreateRoomActionResponse(message);
                        break;
                    case ActionEnum.channel_created:
                        await ChannelCreatedNotificationActionResponse(message);
                        break;
                    case ActionEnum.channel_deleted:
                        await ChannelDeletedNotificationActionResponse(message);
                        break;
                    case ActionEnum.enter_room:
                        await EnterRoomActionResponse(message);
                        break;
                    case ActionEnum.leave_room:
                        await LeaveRoomActionResponse(message);
                        break;
                    case ActionEnum.connection_check:
                        await ConnectionCheckActionRequest(message);
                        break;
                    case ActionEnum.get_connection_check_interval:
                        await GetConnectionCheckInterval();
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Something gone wrong in Process !!!");
                Console.WriteLine(e.Message);
                await LogoutActionResponse();
            }
        }

        private async Task AESHandshakeWithRSAActionResponse()
        {
            try
            {
                RSAParameters clientPublicKey = (RSAParameters)(await messageStream.ReadAsync()).Obj;

                byte[] aesKey = Aes.Create().Key;
                byte[] encryptedAesKey;

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    rsa.ImportParameters(clientPublicKey);
                    encryptedAesKey = rsa.Encrypt(aesKey, true);
                }

                await messageStream.WriteAsync(new NetworkMessage(ActionEnum.ok, encryptedAesKey));
                ActionEnum requestResult = (await messageStream.ReadAsync()).Action;

                if (requestResult == ActionEnum.ok)
                    messageStream.Aes.Key = aesKey;

                Console.WriteLine("AES handshake occurred {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("AES handshake exception {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
            }
        }

        private async Task ChannelDeletedNotificationActionResponse(NetworkMessage request)
        {
            try
            {
                request.Obj = ActionQueue.DeletedChannels.Dequeue();
                await messageStream.WriteAsync(request);
            }
            catch (Exception e)
            {
                Console.WriteLine("ChannelDeletedNotification exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
                await LogoutActionResponse();
            }
        }

        private async Task ChannelCreatedNotificationActionResponse(NetworkMessage request)
        {
            try
            {
                request.Obj = ActionQueue.NewChannels.Dequeue();
                await messageStream.WriteAsync(request);
            }
            catch (Exception e)
            {
                Console.WriteLine("ChannelCreatedNotification exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
                await LogoutActionResponse();
            }
        }

        private async Task GetRoomHistoryActionResponse(NetworkMessage message)
        {
            try
            {
                string roomName = (string)message.Obj;

                List<Message> history = DB.GetChannelHistory(roomName).Result;
                NetworkMessage response = new NetworkMessage(ActionEnum.ok, history);
                await messageStream.WriteEncryptedAsync(response);

                Console.WriteLine("GetRoomHistory ({2}) send to {0} ({1})",
                    Name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(),
                    roomName);
            }
            catch (Exception e)
            {
                Console.WriteLine("GetChannelsActionResponse exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
                await LogoutActionResponse();
            }
        }

        private async Task LeaveRoomActionResponse(NetworkMessage message)
        {
            try
            {
                string roomName = (string)message.Obj;

                ActionEnum res = DB.ExitRoom(Name, roomName).Result;

                NetworkMessage response = new NetworkMessage(res);
                await messageStream.WriteEncryptedAsync(response);
                server.RemoveOnlineRoomUser(this, roomName);

                if (res == ActionEnum.ok)
                {
                    Console.WriteLine("User ({0}) from {1} left room ({2})",
                        Name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(),
                        roomName);
                }
                else
                {
                    Console.WriteLine("User ({0}) from {1} left room ({2}), with exception",
                        Name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(),
                        roomName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("EnterRoom exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
                await LogoutActionResponse();
            }
        }

        private async Task EnterRoomActionResponse(NetworkMessage message)
        {
            try
            {
                string roomName = ((Room)message.Obj).Name;
                string pass = ((Room)message.Obj).HashPass;

                ActionEnum authorizationResult = DB.EnterRoom(Name, roomName, pass).Result;
                NetworkMessage response = new NetworkMessage(authorizationResult);
                await messageStream.WriteEncryptedAsync(response);

                if (authorizationResult == ActionEnum.ok) 
                {
                    server.AddedOnlineRoomUser(this, roomName);

                    Console.WriteLine("User ({0}) from {1} entered room ({2})",
                        Name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(),
                        roomName);
                }
                else
                {
                    Console.WriteLine("User ({0}) from {1} not entered room ({2}), wrong pass",
                        Name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(),
                        roomName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("EnterRoom exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
                await LogoutActionResponse();
            }
        }

        private async Task GetAllHistoryActionResponse()
        {
            try
            {
                List<Message> history = DB.GetAllHistory(Name).Result;
                NetworkMessage response = new NetworkMessage(ActionEnum.ok, history);
                await messageStream.WriteEncryptedAsync(response);

                Console.WriteLine("GetHistory send to {0} ({1})",
                    Name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("GetChannelsActionResponse exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
                await LogoutActionResponse();
            }
        }

        private async Task GetChannelsActionResponse()
        {
            try
            {
                List<Channel> channels = DB.GetChannels(Name).Result;
                NetworkMessage response = new NetworkMessage(ActionEnum.ok, channels);
                await messageStream.WriteEncryptedAsync(response);

                server.AddedOnlineRoomsUser(this, channels);

                Console.WriteLine("GetChannels send to {0} ({1})",
                    Name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("GetChannelsActionResponse exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
                await LogoutActionResponse();
            }
        }
        private async Task GetConnectionCheckInterval()
        {
            try
            {
                NetworkMessage response = new NetworkMessage(ActionEnum.ok,
                    server.ConnectedIpConnectionCheckTimerInterval);
                await messageStream.WriteAsync(response);

                Console.WriteLine("GetConnectionCheckInterval send to {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("GetConnectionCheckInterval exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
                await LogoutActionResponse();
            }
        }

        private async Task ConnectionCheckActionRequest(NetworkMessage message)
        {
            try
            {
                await messageStream.WriteAsync(message);
                await messageStream.ReadAsync();
                ActionQueue.ConnectionCheckRequired = false;
               // Console.WriteLine("connection check");
            }
            catch (Exception e)
            {
                Console.WriteLine("ConnectionCheck exception - {0}({1})",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(), Name);
                Console.WriteLine(e.Message);
                await LogoutActionResponse();
            }
        }

        private async Task SendMessageActionResponse(NetworkMessage request)
        {
            try
            {
                Message cur = ActionQueue.Messages.Dequeue();
                NetworkMessage message = new NetworkMessage(ActionEnum.receive_message, cur);
                await messageStream.WriteEncryptedAsync(message);

                Console.WriteLine("Message send from {0} to {1}", cur.From, cur.To);

            }
            catch (Exception e)
            {
                Console.WriteLine("Send message exception - {0} ({1}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(), e.Message);
                await LogoutActionResponse();
            }
        }

        private async Task ReceiveMessageActionResponse(NetworkMessage message)
        {
            try
            {
                Console.WriteLine("Message received from {0}", Name);

                Message curMessage = (Message)message.Obj;

                bool isMessageToRoom = await DB.IsRoom(curMessage.To);
                bool res = false;

                if (isMessageToRoom && await DB.SendToRoom(curMessage))
                {
                    server.SendToOnlineClientsRoom(curMessage);
                    res = true;
                }
                else if (await DB.SendToUser(curMessage))
                {
                    server.SendToOnliceClient(curMessage);
                    res = true;
                }


                if (res)
                {
                    NetworkMessage response = new NetworkMessage(ActionEnum.ok);
                    await messageStream.WriteAsync(response);
                }
                else
                {
                    NetworkMessage response = new NetworkMessage(ActionEnum.bad);
                    await messageStream.WriteAsync(response);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Receive message exception - {0} ({1})",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(), e.Message);
                await LogoutActionResponse();
            }
        }

        private async Task CreateRoomActionResponse(NetworkMessage request)
        {
            try
            {
                Console.WriteLine("User {0} try to create room",
                    ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString());

                string roomName = ((Room)request.Obj).Name;
                string hashPass = ((Room)request.Obj).HashPass;
                string salt = null;
                if (!string.IsNullOrWhiteSpace(hashPass))
                {
                    salt = CreateSalt();
                    hashPass += salt;
                }

                ActionEnum responseAction = await DB.CreateRoom(roomName, Name, hashPass, salt);

                if (responseAction == ActionEnum.ok)
                {
                    Console.WriteLine("User {0} from {1} created room {2} ",
                        Name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(),
                        roomName);

                    Channel newChannel = new Channel()
                    {
                        Name = roomName,
                        Type = string.IsNullOrWhiteSpace(hashPass) ? ChannelType.public_open : ChannelType.public_closed,
                    };
                    server.ChannelCreatedNotify(newChannel, Name);
                    server.AddedOnlineRoomUser(this, roomName);
                }
                else if (responseAction == ActionEnum.room_exist)
                {
                    Console.WriteLine("User {0} from {1} haven't created room, the channel {2} exist",
                        Name, ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString(),
                        roomName);
                }
                await messageStream.WriteAsync(new NetworkMessage(responseAction));
            }
            catch (Exception e)
            {

                Console.WriteLine("Room creation exception - {0} ({1})",
                   ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(), e.Message);

                await LogoutActionResponse();
            }
        }

        private async Task CreateUserActionResponse(NetworkMessage request)
        {
            try
            {
                Console.WriteLine("User {0} try to create account ",
                    ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString());

                string login = ((User)request.Obj).Login;
                string hashPass = ((User)request.Obj).HashPass;
                string salt = CreateSalt();
                hashPass += salt;

                bool res = await DB.CreateUser(login, hashPass, salt);
                if (res)
                {
                    Console.WriteLine("User {0} from {1} created ",
                        login, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());

                    Name = login;
                    server.ChannelCreatedNotify(new Channel() { Name = login, Type = ChannelType.user }, Name);
                    NetworkMessage response = new NetworkMessage(ActionEnum.ok);
                    await messageStream.WriteAsync(response);
                }
                else
                {
                    Console.WriteLine("User {0} from {1} is not created, db issue occures",
                        login, ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString());

                    NetworkMessage response = new NetworkMessage(ActionEnum.bad);
                    await messageStream.WriteAsync(response);
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("User creation exception - {0} ({1})",
                   ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(), e.Message);

                await LogoutActionResponse();
            }
        }

        private async Task LoginActionResponse(NetworkMessage message)
        {
            try
            {
                Console.WriteLine("User {0} try to login ",
                    ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString());

                string login = ((User)message.Obj).Login;
                string hashPass = ((User)message.Obj).HashPass;

                // bool userAlreadyOnline = !server.UserExist(login);

                ActionEnum responseActionEnum = await DB.UserLogin(login, hashPass);

                if (responseActionEnum == ActionEnum.bad_login)
                {
                    Console.WriteLine("User {0} from {1} not exist ",
                        login, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                }
                else if (responseActionEnum == ActionEnum.wrong_pass)
                {
                    Console.WriteLine("User {0} from {1} wrong password ",
                        login, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                }
                else if (responseActionEnum == ActionEnum.room_exist)
                {
                    Console.WriteLine("User {0} from {1} try to login as room",
                        login, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                }
                else if (responseActionEnum == ActionEnum.ok)
                {
                    Console.WriteLine("User {0} from {1} Logged in",
                        login, ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString());

                    Name = login;
                }

                await messageStream.WriteAsync(new NetworkMessage(responseActionEnum));
            }
            catch (Exception e)
            {

                Console.WriteLine("Login exception - {0} ({1})",
                   ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(), e.Message);

                await LogoutActionResponse();
            }
        }

        private async Task LogoutActionResponse()
        {
            try
            {
                Console.WriteLine("User {0} from {1} Logged out and disconnected",
                    Name, ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString());

                TcpClient.Close();
                streamIsOpen = false;
                server.RemoveOnlineRoomsUser(this);
                server.RemoveFromConnectedIp(this);
            }
            catch (Exception e)
            {
                Console.WriteLine("Something gone wrong in LogoutActionResponse !!!");
                Console.WriteLine(e.Message);
            }
        }

        private string CreateSalt()
        {
            Random rand = new Random();
            StringBuilder saltBuilder = new StringBuilder();
            saltBuilder.Length = rand.Next(10, 25);
            for (int i = 0; i < saltBuilder.Length; i++)
                saltBuilder[i] = (char)rand.Next(48, 126);
            return saltBuilder.ToString();
        }
    }
}
