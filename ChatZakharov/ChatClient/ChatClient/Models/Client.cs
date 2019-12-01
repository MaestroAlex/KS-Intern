using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TransitPackage;

namespace ChatClient.Models
{
    public enum ClientState
    {
        Connecting, Connected, Disconnected, LoggedIn, LoggedOut
    }
    public class Client : ViewModelBase, IDisposable
    {
        private ClientState connectionState;
        public ClientState ConnectionState
        {
            get => connectionState;
            set => Set(ref connectionState, value);
        }

        private Config config;
        public string Name { get; private set; }
        public event EventHandler<Message> MessageReceived;
        TcpClient tcpClient;
        NetworkStream stream;
        NetworkMessageStream messageStream;
        bool streamIsOpen;
        System.Timers.Timer connnectionCheckForClientTimer;
        System.Timers.Timer changeAesKeyTimer;
        bool connectionCheckForClientExist;
        SemaphoreSlim NetworkStreamSemaphore; // флаг, разрешающий чтение в потоке ожидания обновления данных 
                                  // (иначе он может вмешиваться в работу NetworkStream при реквестах)

        public Client(Config config)
        {
            Name = null;
            ConnectionState = ClientState.Disconnected;
            NetworkStreamSemaphore = new SemaphoreSlim(01);

            connnectionCheckForClientTimer = new System.Timers.Timer(); // .Interval sets in GetConnectionChekInterval()
            connnectionCheckForClientTimer.Elapsed += (sender, e) =>
            {
                if (connectionCheckForClientExist)
                    connectionCheckForClientExist = false;
                else
                    Disconnect();
            };

            changeAesKeyTimer = new System.Timers.Timer();
            changeAesKeyTimer.Elapsed += (sender, e) => AESHandshakeWithRSAActionRequest();
            changeAesKeyTimer.Interval = 7200000; // aes keys change every 2 hours
            this.config = config;
        }

        public void StartListening()
        {
            Task.Run(() => FromServerReceiveTask());
        }

        private async Task FromServerReceiveTask()
        {
            while (streamIsOpen)
            {
                await NetworkStreamSemaphore.WaitAsync();
                if (streamIsOpen && stream.DataAvailable)
                {
                    NetworkMessage networkMessage = await messageStream.ReadAsync();
                    if (await Process(networkMessage))
                        connectionCheckForClientExist = true;

                }
                NetworkStreamSemaphore.Release();
                await Task.Delay(100);
            }
        }

        private async Task<bool> Process(NetworkMessage networkMessage)
        {
            switch (networkMessage.Action)
            {
                case ActionEnum.receive_message:
                    return ReceiveMessageActionResponse(networkMessage);
                case ActionEnum.channel_created:
                    return ChannelCreatedNotificationActionResponse(networkMessage);
                case ActionEnum.channel_deleted:
                    return ChannelDeletedNotificationActionResponse(networkMessage);
                case ActionEnum.connection_check:
                    return await ConnectionCheckActionResponse(networkMessage);
            }
            return false;
        }

        private bool ChannelDeletedNotificationActionResponse(NetworkMessage message)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(
                    new Func<Channel, bool>(MainModel.ConnectedChannels.Remove), message.Obj);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("UserLoggedIn exception");
                Console.WriteLine(e.Message);

                return false;
            }
        }

        private bool ChannelCreatedNotificationActionResponse(NetworkMessage message)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(
                    new Action<Channel>(MainModel.ConnectedChannels.Add), message.Obj);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("UserLoggedIn exception");
                Console.WriteLine(e.Message);

                return false;
            }
        }

        private async Task<bool> ConnectionCheckActionResponse(NetworkMessage message)
        {
            try
            {
                await messageStream.WriteAsync(message);

                return true;
            }
            catch (Exception ex)
            {
                Disconnect();
                Console.WriteLine(ex.Message);
                Console.WriteLine("ConnectionCheck - bad");

                return false;
            }
        }

        private bool ReceiveMessageActionResponse(NetworkMessage message)
        {
            try
            {
                MessageReceived?.Invoke(this, (Message)message.Obj);
                Console.WriteLine("Message received");

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Error in Message received");

                return false;
            }
        }

        public async Task<bool> SendMessageActionRequest(Message message)
        {
            await NetworkStreamSemaphore.WaitAsync();

            bool res = false;
            try
            {
                NetworkMessage request = new NetworkMessage(ActionEnum.receive_message, message);
                await messageStream.WriteEncryptedAsync(request);

                ActionEnum requsetResult = (await messageStream.ReadAsync()).Action;

                connectionCheckForClientExist = true;

                if (requsetResult == ActionEnum.ok)
                {
                    Console.WriteLine("Message send");
                    res = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Error in Message send");
                Disconnect();
                res = false;
            }
            finally
            {
                NetworkStreamSemaphore.Release();
            }
            return res;
        }

        public async Task<bool> GetConnectionChekInterval()
        {
            await NetworkStreamSemaphore.WaitAsync();
            bool res;

            try
            {
                NetworkMessage request = new NetworkMessage(ActionEnum.get_connection_check_interval);
                await messageStream.WriteAsync(request);
                connnectionCheckForClientTimer.Interval = (int)(await messageStream.ReadAsync()).Obj + 2000;
                connnectionCheckForClientTimer.Start();
                res = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Disconnect();
                res = false;
            }
            finally
            {
                NetworkStreamSemaphore.Release();
            }
            return res;
        }

        public async Task GetRoomHistoryActionRequest(string roomName)
        {
            await NetworkStreamSemaphore.WaitAsync();

            try
            {
                NetworkMessage request = new NetworkMessage(ActionEnum.get_room_history, roomName);
                await messageStream.WriteEncryptedAsync(request);

                NetworkMessage response = await messageStream.ReadAsync();

                if (response.Action == ActionEnum.ok)
                {
                    List<Message> history = (List<Message>)response.Obj;
                    foreach (var item in history)
                        MessageReceived?.Invoke(this, item);

                    connectionCheckForClientExist = true;
                    Console.WriteLine("History received");
                }
                else
                    throw new Exception("Something wrong on server side");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Error in History received");
                Disconnect();
            }
            finally
            {
                NetworkStreamSemaphore.Release();
            }
        }

        public async Task<bool> LeaveRoomActionRequest(object room)
        {
            await NetworkStreamSemaphore.WaitAsync();

            bool res = false;
            try
            {
                NetworkMessage request = new NetworkMessage(ActionEnum.leave_room, room);
                await messageStream.WriteEncryptedAsync(request);

                NetworkMessage response = await messageStream.ReadAsync();
                if (response.Action == ActionEnum.ok)
                {
                    connectionCheckForClientExist = true;
                    res = true;
                }
                else
                    throw new Exception("Something wrong on server side");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Disconnect();
                res = false;
            }
            finally
            {
                NetworkStreamSemaphore.Release();
            }
            return res;
        }

        public async Task<ActionEnum> EnterRoomActionRequest(Channel channel, string hashPassword)
        {
            await NetworkStreamSemaphore.WaitAsync();
            ActionEnum res;
            try
            {
                Room room = new Room
                {
                    Name = channel.Name,
                    HashPass = hashPassword
                };
                NetworkMessage request = new NetworkMessage(ActionEnum.enter_room, room);
                await messageStream.WriteEncryptedAsync(request);

                NetworkMessage response = await messageStream.ReadAsync();
                res = response.Action;
                connectionCheckForClientExist = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                res = ActionEnum.bad;
                Disconnect();
            }
            finally
            {
                NetworkStreamSemaphore.Release();
            }
            return res;
        }

        public async Task GetAllHistoryActionRequest()
        {
            await NetworkStreamSemaphore.WaitAsync();

            try
            {
                NetworkMessage request = new NetworkMessage(ActionEnum.get_all_history);
                await messageStream.WriteAsync(request);

                NetworkMessage response = await messageStream.ReadAsync();
                if (response.Action == ActionEnum.ok)
                {
                    List<Message> history = (List<Message>)response.Obj;
                    foreach (var item in history)
                        MessageReceived?.Invoke(this, item);

                    connectionCheckForClientExist = true;
                    Console.WriteLine("History received");
                }
                else
                    throw new Exception("Something wrong on server side");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Error in History received");
                Disconnect();
            }
            finally
            {
                NetworkStreamSemaphore.Release();
            }
        }

        public async Task<IEnumerable<Channel>> GetChannelsActionRequest()
        {
            await NetworkStreamSemaphore.WaitAsync();
            IEnumerable<Channel> res = null;

            try
            {
                NetworkMessage request = new NetworkMessage(ActionEnum.get_channels);
                await messageStream.WriteAsync(request);

                NetworkMessage response = await messageStream.ReadAsync();
                res = (List<Channel>)response.Obj;
                Console.WriteLine("GetUsers receive");

                connectionCheckForClientExist = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Disconnect();
            }
            finally
            {
                NetworkStreamSemaphore.Release();
            }
            return res;
        }

        public async Task<bool> AESHandshakeWithRSAActionRequest()
        {
            await NetworkStreamSemaphore.WaitAsync();

            bool res;
            try
            {
                await messageStream.WriteAsync(new NetworkMessage(ActionEnum.aes_handshake));

                RSAParameters clientPublicKey;
                RSAParameters clientPrivateKey;

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    clientPublicKey = rsa.ExportParameters(false);
                    clientPrivateKey = rsa.ExportParameters(true);
                }

                await messageStream.WriteAsync(new NetworkMessage(ActionEnum.ok, clientPublicKey));

                byte[] encryptedAesKey = (byte[])(await messageStream.ReadAsync()).Obj;
                byte[] decryptedAesKey;

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    rsa.ImportParameters(clientPrivateKey);
                    decryptedAesKey = rsa.Decrypt(encryptedAesKey, true);
                }

                messageStream.Aes.Key = decryptedAesKey;
                await messageStream.WriteAsync(new NetworkMessage(ActionEnum.ok));
                res = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("AES handshake exception");
                Console.WriteLine(e.Message);
                Disconnect();
                res = false;
            }
            finally
            {
                NetworkStreamSemaphore.Release();
            }
            return res;
        }

        public async Task<ActionEnum> LoginActionRequest(string login, string hashPassword = null)
        {
            await NetworkStreamSemaphore.WaitAsync();
            ActionEnum response;

            try
            {
                User user = new User() { Login = login, HashPass = hashPassword };
                NetworkMessage request = new NetworkMessage(ActionEnum.login, user);
                await messageStream.WriteEncryptedAsync(request);

                response = (await messageStream.ReadAsync()).Action;

                if (response == ActionEnum.ok)
                {
                    Name = login;
                    ConnectionState = ClientState.LoggedIn;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Disconnect();
                response = ActionEnum.bad;
            }
            finally
            {
                NetworkStreamSemaphore.Release();
            }
            return response;
        }

        public async Task<ActionEnum> CreateNewRoomActionRequest(string roomName, string hashPassword)
        {
            await NetworkStreamSemaphore.WaitAsync();
            ActionEnum response;

            try
            {
                Room room = new Room() { Name = roomName, HashPass = hashPassword };
                NetworkMessage request = new NetworkMessage(ActionEnum.create_room, room);
                await messageStream.WriteEncryptedAsync(request);
                response = (await messageStream.ReadAsync()).Action;

                connectionCheckForClientExist = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Disconnect();
                response = ActionEnum.bad;
            }

            NetworkStreamSemaphore.Release();
            return response;
        }

        public async Task<ActionEnum> CreateNewUserActionRequest(string login, string hashPassword)
        {
            await NetworkStreamSemaphore.WaitAsync();
            ActionEnum response;

            try
            {
                User user = new User() { Login = login, HashPass = hashPassword };
                NetworkMessage request = new NetworkMessage(ActionEnum.create_user, user);
                await messageStream.WriteEncryptedAsync(request);

                response = (await messageStream.ReadAsync()).Action;

                if (response == ActionEnum.ok)
                {
                    Name = login;
                    ConnectionState = ClientState.LoggedIn;
                }

                connectionCheckForClientExist = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Disconnect();
                response = ActionEnum.bad;
            }

            NetworkStreamSemaphore.Release();
            return response;
        }

        private bool inConnect;
        private bool exitConnect;
        public async Task<bool> Connect()
        {
            await NetworkStreamSemaphore.WaitAsync();
            bool res = false;

            if (!inConnect)
                res = TryToConnect();

            NetworkStreamSemaphore.Release();
            return res;
        }
        private bool TryToConnect()
        {
            inConnect = true;
            try
            {
                ConnectionState = ClientState.Connecting;

                bool connect = false;
                for (int connectionTry = 0; connectionTry < 20; connectionTry++)
                {
                    connect = InternalTryToConnect();
                    if (exitConnect || connect)
                        break;
                }
                if (connect)
                {
                    tcpClient.ReceiveTimeout = config.ConnectionTimeoutMs;
                    tcpClient.SendTimeout = config.ConnectionTimeoutMs;
                    stream = tcpClient.GetStream();
                    messageStream = new NetworkMessageStream(stream);
                    streamIsOpen = true;
                    changeAesKeyTimer.Start();
                    StartListening();
                    ConnectionState = ClientState.Connected;

                    inConnect = false;
                    exitConnect = false;
                    return true;
                }
                else
                    ConnectionState = ClientState.Disconnected;
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection Exception");
                Console.WriteLine(e.Message);
            }

            exitConnect = false;
            inConnect = false;
            return false;
        }

        private bool InternalTryToConnect()
        {
            tcpClient = new TcpClient();
            IAsyncResult ar = tcpClient.BeginConnect(config.Ip, config.Port, null, null);
            WaitHandle wh = ar.AsyncWaitHandle;
            try
            {
                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(500), false))
                {
                    tcpClient.Close();
                    return false;
                }

                tcpClient.EndConnect(ar);
                return true;
            }
            finally
            {
                wh.Close();
            }
        }

        private void Disconnect()
        {
            ConnectionState = ClientState.Disconnected;
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
                streamIsOpen = false;
                changeAesKeyTimer.Stop();
                connnectionCheckForClientTimer.Stop();
            }
        }

        public async Task LogoutActionRequest()
        {
            exitConnect = true;
            await NetworkStreamSemaphore.WaitAsync();

            try
            {
                NetworkMessage request = new NetworkMessage(ActionEnum.logout);
                await messageStream.WriteAsync(request);
                Disconnect();
                ConnectionState = ClientState.LoggedOut;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            exitConnect = false;
            NetworkStreamSemaphore.Release();
        }

        public void Dispose()
        {
            LogoutActionRequest();
            NetworkStreamSemaphore.Dispose();
        }
    }
}
