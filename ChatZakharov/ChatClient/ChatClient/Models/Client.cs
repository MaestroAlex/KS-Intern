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
        Mutex NetworkStreamMutex; // флаг, разрешающий чтение в потоке ожидания обновления данных 
                                  // (иначе он может вмешиваться в работу NetworkStream при реквестах)

        public Client(Config config)
        {
            Name = null;
            ConnectionState = ClientState.Disconnected;
            NetworkStreamMutex = new Mutex();

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

        private void FromServerReceiveTask()
        {
            while (streamIsOpen)
            {
                NetworkStreamMutex.WaitOne();
                if (streamIsOpen && stream.DataAvailable)
                {
                    NetworkMessage networkMessage = messageStream.Read();
                    if (Process(networkMessage))
                        connectionCheckForClientExist = true;

                }
                NetworkStreamMutex.ReleaseMutex();
                Thread.Sleep(100);
            }
        }

        private bool Process(NetworkMessage networkMessage)
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
                    return ConnectionCheckActionResponse(networkMessage);
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

        private bool ConnectionCheckActionResponse(NetworkMessage message)
        {
            try
            {
                messageStream.Write(message);

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

        public bool SendMessageActionRequest(Message message)
        {
            NetworkStreamMutex.WaitOne();

            //if (Name == "debug")
            //    Console.WriteLine("Enter SendMessage");

            bool res = false;
            try
            {
                NetworkMessage request = new NetworkMessage(ActionEnum.receive_message, message);
                messageStream.WriteEncrypted(request);

                ActionEnum requsetResult = messageStream.Read().Action;

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

            //if (Name == "debug")
            //    Console.WriteLine("Exit SendMessage");

            NetworkStreamMutex.ReleaseMutex();
            return res;
        }

        public bool GetConnectionChekInterval()
        {
            NetworkStreamMutex.WaitOne();
            bool res;

            try
            {
                NetworkMessage request = new NetworkMessage(ActionEnum.get_connection_check_interval);
                messageStream.Write(request);
                connnectionCheckForClientTimer.Interval = (int)messageStream.Read().Obj + 2000;
                connnectionCheckForClientTimer.Start();
                res = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                res = false;
            }

            NetworkStreamMutex.ReleaseMutex();
            return res;
        }

        public void GetHistoryActionRequest()
        {
            try
            {
                NetworkMessage request = new NetworkMessage(ActionEnum.get_history);
                messageStream.Write(request);

                List<Message> history = (List<Message>)messageStream.Read().Obj;
                foreach (var item in history)
                    MessageReceived?.Invoke(this, item);

                Console.WriteLine("History received");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Error in History received");
            }
        }

        public IEnumerable<Channel> GetChannelsActionRequest()
        {
            NetworkStreamMutex.WaitOne();
            IEnumerable<Channel> res = null;

            try
            {
                NetworkMessage request = new NetworkMessage(ActionEnum.get_channels);
                messageStream.Write(request);
                NetworkMessage response = messageStream.Read();
                res = (List<Channel>)response.Obj;
                Console.WriteLine("GetUsers receive");

                connectionCheckForClientExist = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            NetworkStreamMutex.ReleaseMutex();
            return res;
        }

        public bool AESHandshakeWithRSAActionRequest()
        {
            NetworkStreamMutex.WaitOne();

            bool res;
            try
            {
                messageStream.Write(new NetworkMessage(ActionEnum.aes_handshake));

                RSAParameters clientPublicKey;
                RSAParameters clientPrivateKey;

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    clientPublicKey = rsa.ExportParameters(false);
                    clientPrivateKey = rsa.ExportParameters(true);
                }

                messageStream.Write(new NetworkMessage(ActionEnum.ok, clientPublicKey));

                byte[] encryptedAesKey = (byte[])messageStream.Read().Obj;
                byte[] decryptedAesKey;

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    rsa.ImportParameters(clientPrivateKey);
                    decryptedAesKey = rsa.Decrypt(encryptedAesKey, true);
                }

                messageStream.Aes.Key = decryptedAesKey;
                messageStream.Write(new NetworkMessage(ActionEnum.ok));
                res = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("AES handshake exception");
                Console.WriteLine(e.Message);
                res = false;
            }

            NetworkStreamMutex.ReleaseMutex();
            return res;
        }

        public ActionEnum LoginActionRequest(string login, string hashPassword = null)
        {
            NetworkStreamMutex.WaitOne();
            ActionEnum response;

            try
            {
                User user = new User() { Login = login, HashPass = hashPassword };
                NetworkMessage request = new NetworkMessage(ActionEnum.login, user);
                messageStream.WriteEncrypted(request);

                response = messageStream.Read().Action;

                if (response == ActionEnum.ok)
                {
                    Name = login;
                    ConnectionState = ClientState.LoggedIn;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                response = ActionEnum.bad;
                LogoutActionRequest();
            }

            NetworkStreamMutex.ReleaseMutex();
            return response;
        }

        public ActionEnum CreateNewRoomActionRequest(string roomName, string hashPassword)
        {
            NetworkStreamMutex.WaitOne();
            ActionEnum response;

            try
            {
                Room room = new Room() { UserOwner = Name, HashPass = hashPassword, Name = roomName };
                NetworkMessage request = new NetworkMessage(ActionEnum.create_room, room);
                messageStream.WriteEncrypted(request);
                response = messageStream.Read().Action;

                connectionCheckForClientExist = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                response = ActionEnum.bad;
            }

            NetworkStreamMutex.ReleaseMutex();
            return response;
        }

        public ActionEnum CreateNewUserActionRequest(string login, string hashPassword)
        {
            NetworkStreamMutex.WaitOne();
            ActionEnum response;

            try
            {
                User user = new User() { Login = login, HashPass = hashPassword };
                NetworkMessage request = new NetworkMessage(ActionEnum.create_user, user);
                messageStream.WriteEncrypted(request);

                response = messageStream.Read().Action;

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
                response = ActionEnum.bad;
            }

            NetworkStreamMutex.ReleaseMutex();
            return response;
        }

        private bool inConnect;
        private bool exitConnect;
        public bool Connect()
        {
            NetworkStreamMutex.WaitOne();
            bool res = false;

            if (!inConnect)
                res = TryToConnect();

            NetworkStreamMutex.ReleaseMutex();
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
                    tcpClient.ReceiveTimeout = config.ReceiveTimeoutMs;
                    tcpClient.SendTimeout = config.SendTimeoutMs;
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

        public void LogoutActionRequest()
        {
            exitConnect = true;
            NetworkStreamMutex.WaitOne();

            try
            {
                NetworkMessage request = new NetworkMessage(ActionEnum.logout);
                messageStream.Write(request);
                Disconnect();
                ConnectionState = ClientState.LoggedOut;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            exitConnect = false;
            NetworkStreamMutex.ReleaseMutex();
        }

        public void Dispose()
        {
            LogoutActionRequest();
            NetworkStreamMutex.WaitOne();
            NetworkStreamMutex.Dispose();
        }
    }
}
