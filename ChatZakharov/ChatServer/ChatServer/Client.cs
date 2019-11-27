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

        public void DefineAction()
        {
            try
            {
                while (streamIsOpen)
                {
                    if (networkStream.DataAvailable)
                    {
                        IsBusy = true;
                        NetworkMessage request = messageStream.Read();
                        Process(request);
                        IsBusy = false;
                    }
                    else if (ActionQueue.Messages.Count != 0)
                    {
                        NetworkMessage response = new NetworkMessage(ActionEnum.send_message);
                        Process(response);
                    }
                    else if (ActionQueue.NewChannels.Count != 0)
                    {
                        NetworkMessage response = new NetworkMessage(ActionEnum.channel_created);
                        Process(response);
                    }
                    else if (ActionQueue.DeletedChannels.Count != 0)
                    {
                        NetworkMessage response = new NetworkMessage(ActionEnum.channel_deleted);
                        Process(response);
                    }
                    else if (ActionQueue.ConnectionCheckRequired)
                    {
                        NetworkMessage response = new NetworkMessage(ActionEnum.connection_check);
                        Process(response);
                    }

                    Thread.Sleep(100);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Something gone wrong in DefineAction !!!");
                Console.WriteLine(e.Message);
                LogoutActionResponse();
            }
        }

        private void Process(NetworkMessage message)
        {
            try
            {
                switch (message.Action)
                {
                    case ActionEnum.aes_handshake:
                        AESHandshakeWithRSAActionResponse();
                        break;
                    case ActionEnum.login:
                        LoginActionResponse(message);
                        break;
                    case ActionEnum.logout:
                        LogoutActionResponse();
                        break;
                    case ActionEnum.send_message:
                        SendMessageActionResponse(message);
                        break;
                    case ActionEnum.receive_message:
                        ReceiveMessageActionResponse(message);
                        break;
                    case ActionEnum.get_channels:
                        GetChannelsActionResponse();
                        break;
                    case ActionEnum.get_all_history:
                        GetAllHistoryActionResponse();
                        break;
                    case ActionEnum.get_room_history:
                        GetRoomHistoryActionResponse(message);
                        break;
                    case ActionEnum.create_user:
                        CreateUserActionResponse(message);
                        break;
                    case ActionEnum.create_room:
                        CreateRoomActionResponse(message);
                        break;
                    case ActionEnum.channel_created:
                        ChannelCreatedNotificationActionResponse(message);
                        break;
                    case ActionEnum.channel_deleted:
                        ChannelDeletedNotificationActionResponse(message);
                        break;
                    case ActionEnum.enter_room:
                        EnterRoomActionResponse(message);
                        break;
                    case ActionEnum.connection_check:
                        ConnectionCheckActionRequest(message);
                        break;
                    case ActionEnum.get_connection_check_interval:
                        GetConnectionCheckInterval();
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Something gone wrong in Process !!!");
                Console.WriteLine(e.Message);
                LogoutActionResponse();
            }
        }

        private void AESHandshakeWithRSAActionResponse()
        {
            try
            {
                RSAParameters clientPublicKey = (RSAParameters)messageStream.Read().Obj;

                byte[] aesKey = Aes.Create().Key;
                byte[] encryptedAesKey;

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    rsa.ImportParameters(clientPublicKey);
                    encryptedAesKey = rsa.Encrypt(aesKey, true);
                }

                messageStream.Write(new NetworkMessage(ActionEnum.ok, encryptedAesKey));
                ActionEnum requestResult = messageStream.Read().Action;

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

        private void ChannelDeletedNotificationActionResponse(NetworkMessage request)
        {
            try
            {
                request.Obj = ActionQueue.DeletedChannels.Dequeue();
                messageStream.Write(request);
            }
            catch (Exception e)
            {
                Console.WriteLine("ChannelDeletedNotification exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
                LogoutActionResponse();
            }
        }

        private void ChannelCreatedNotificationActionResponse(NetworkMessage request)
        {
            try
            {
                request.Obj = ActionQueue.NewChannels.Dequeue();
                messageStream.Write(request);
            }
            catch (Exception e)
            {
                Console.WriteLine("ChannelCreatedNotification exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
                LogoutActionResponse();
            }
        }

        private void GetRoomHistoryActionResponse(NetworkMessage message)
        {
            try
            {
                string roomName = (string)message.Obj;

                List<Message> history = DB.GetChannelHistory(roomName).Result;
                NetworkMessage response = new NetworkMessage(ActionEnum.ok, history);
                messageStream.WriteEncrypted(response);

                Console.WriteLine("GetRoomHistory ({2}) send to {0} ({1})",
                    Name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(),
                    roomName);
            }
            catch (Exception e)
            {
                Console.WriteLine("GetChannelsActionResponse exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
            }
        }

        private void EnterRoomActionResponse(NetworkMessage message)
        {
            try
            {
                string roomName = ((Room)message.Obj).Name;
                string pass = ((Room)message.Obj).HashPass;

                ActionEnum authorizationResult = DB.EnterRoom(Name, roomName, pass).Result;
                NetworkMessage response = new NetworkMessage(authorizationResult);
                messageStream.WriteEncrypted(response);

                Console.WriteLine("User ({0}) from {1} entered room ({2})",
                    Name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(),
                    roomName);
            }
            catch (Exception e)
            {
                Console.WriteLine("EnterRoom exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
            }
        }

        private void GetAllHistoryActionResponse()
        {
            try
            {
                List<Message> history = DB.GetAllHistory(Name).Result;
                NetworkMessage response = new NetworkMessage(ActionEnum.ok, history);
                messageStream.WriteEncrypted(response);

                Console.WriteLine("GetHistory send to {0} ({1})",
                    Name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("GetChannelsActionResponse exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
            }
        }

        private void GetChannelsActionResponse()
        {
            try
            {
                List<Channel> channels = DB.GetChannels(Name).Result;
                NetworkMessage response = new NetworkMessage(ActionEnum.ok, channels);
                messageStream.WriteEncrypted(response);

                Console.WriteLine("GetChannels send to {0} ({1})",
                    Name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("GetChannelsActionResponse exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
            }
        }
        private void GetConnectionCheckInterval()
        {
            try
            {
                NetworkMessage response = new NetworkMessage(ActionEnum.ok,
                    server.ConnectedIpConnectionCheckTimerInterval);
                messageStream.Write(response);

                Console.WriteLine("GetConnectionCheckInterval send to {0} ({1})",
                    Name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("GetConnectionCheckInterval exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
                LogoutActionResponse();
            }
        }

        private void ConnectionCheckActionRequest(NetworkMessage message)
        {
            try
            {
                messageStream.Write(message);
                messageStream.Read();
                ActionQueue.ConnectionCheckRequired = false;
               // Console.WriteLine("connection check");
            }
            catch (Exception e)
            {
                Console.WriteLine("ConnectionCheck exception - {0}({1})",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(), Name);
                Console.WriteLine(e.Message);
                LogoutActionResponse();
            }
        }

        private void SendMessageActionResponse(NetworkMessage request)
        {
            try
            {
                Message cur = ActionQueue.Messages.Dequeue();
                NetworkMessage message = new NetworkMessage(ActionEnum.receive_message, cur);
                messageStream.WriteEncrypted(message);

                Console.WriteLine("Message send from {0} to {1}", cur.From, cur.To);

            }
            catch (Exception e)
            {
                Console.WriteLine("Send message exception - {0} ({1}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(), e.Message);
            }
        }

        private void ReceiveMessageActionResponse(NetworkMessage message)
        {
            try
            {
                Console.WriteLine("Message received from {0}", Name);

                bool isMessageToRoom = DB.IsRoom(((Message)message.Obj).To).Result;
                bool res = false;

                if (isMessageToRoom && DB.SendToRoom(message).Result)
                {
                    server.SendToOnlineClients((Message)message.Obj);
                    res = true;
                }
                else if (DB.SendToUser(message).Result)
                {
                    server.SendToOnliceClient((Message)message.Obj);
                    res = true;
                }


                if (res)
                {
                    NetworkMessage response = new NetworkMessage(ActionEnum.ok);
                    messageStream.Write(response);
                }
                else
                {
                    NetworkMessage response = new NetworkMessage(ActionEnum.bad);
                    messageStream.Write(response);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Receive message exception - {0} ({1})",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(), e.Message);
            }
        }

        private void CreateRoomActionResponse(NetworkMessage request)
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

                ActionEnum responseAction = DB.CreateRoom(roomName, Name, hashPass, salt).Result;

                if (responseAction == ActionEnum.ok)
                {
                    Console.WriteLine("User {0} from {1} created room {2} ",
                        Name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(),
                        roomName);

                    server.ChannelCreatedNotify(new Channel() { Name = roomName, Type = hashPass == "" ? ChannelType.public_open : ChannelType.public_closed });
                }
                else if (responseAction == ActionEnum.room_exist)
                {
                    Console.WriteLine("User {0} from {1} haven't created room, the channel {2} exist",
                        Name, ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString(),
                        roomName);
                }
                messageStream.Write(new NetworkMessage(responseAction));
            }
            catch (Exception e)
            {

                Console.WriteLine("Room creation exception - {0} ({1})",
                   ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(), e.Message);

                LogoutActionResponse();
            }
        }

        private void CreateUserActionResponse(NetworkMessage request)
        {
            try
            {
                Console.WriteLine("User {0} try to create account ",
                    ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString());

                string login = ((User)request.Obj).Login;
                string hashPass = ((User)request.Obj).HashPass;
                string salt = CreateSalt();
                hashPass += salt;

                bool res = DB.CreateUser(login, hashPass, salt).Result;
                if (res)
                {
                    Console.WriteLine("User {0} from {1} created ",
                        login, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());

                    Name = login;
                    server.ChannelCreatedNotify(new Channel() { Name = login, Type = ChannelType.user });
                    NetworkMessage response = new NetworkMessage(ActionEnum.ok);
                    messageStream.Write(response);
                }
                else
                {
                    Console.WriteLine("User {0} from {1} is not created, db issue occures",
                        login, ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString());

                    NetworkMessage response = new NetworkMessage(ActionEnum.bad);
                    messageStream.Write(response);
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("User creation exception - {0} ({1})",
                   ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(), e.Message);

                LogoutActionResponse();
            }
        }

        private void LoginActionResponse(NetworkMessage message)
        {
            try
            {
                Console.WriteLine("User {0} try to login ",
                    ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString());

                string login = ((User)message.Obj).Login;
                string hashPass = ((User)message.Obj).HashPass;

                // bool userAlreadyOnline = !server.UserExist(login);

                ActionEnum responseActionEnum = DB.UserLogin(login, hashPass).Result;

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

                messageStream.Write(new NetworkMessage(responseActionEnum));
            }
            catch (Exception e)
            {

                Console.WriteLine("Login exception - {0} ({1})",
                   ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(), e.Message);

                LogoutActionResponse();
            }
        }

        private void LogoutActionResponse()
        {
            try
            {
                Console.WriteLine("User {0} from {1} Logged out and disconnected",
                    Name, ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString());

                TcpClient.Close();
                streamIsOpen = false;
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
