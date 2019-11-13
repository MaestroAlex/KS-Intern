using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
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
        Mutex NetworkStreamMutex; // флаг, разрешающий чтение в потоке ожидания обновления данных 
                                  // (иначе он может вмешиваться в работу NetworkStream при реквестах)

        public Client(Config config)
        {
            Name = null;
            ConnectionState = ClientState.Disconnected;
            NetworkStreamMutex = new Mutex();
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
                    Process(networkMessage);
                }
                NetworkStreamMutex.ReleaseMutex();
                Thread.Sleep(100);
            }
        }

        private void Process(NetworkMessage networkMessage)
        {
            switch (networkMessage.Action)
            {
                case ActionEnum.receive_message:
                    ReceiveMessageActionResponse(networkMessage);
                    break;
                case ActionEnum.user_logged_in:
                    UserLoggedInNotificationActionResponse(networkMessage);
                    break;
                case ActionEnum.user_logged_out:
                    UserLoggedOutNotificationActionResponse(networkMessage);
                    break;
                case ActionEnum.connection_check:
                    ConnectionCheckActionResponse(networkMessage);
                    break;
            }
        }

        private void UserLoggedOutNotificationActionResponse(NetworkMessage message)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(
                    new Func<string, bool>(MainModel.ConnectedUsers.Remove), message.Obj.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("UserLoggedIn exception");
                Console.WriteLine(e.Message);
            }
        }

        private void UserLoggedInNotificationActionResponse(NetworkMessage message)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(
                    new Action<string>(MainModel.ConnectedUsers.Add), message.Obj.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("UserLoggedIn exception");
                Console.WriteLine(e.Message);
            }
        }

        private void ConnectionCheckActionResponse(NetworkMessage message)
        {
            if (Name == "debug")
                Console.WriteLine("Enter ConnectionCheck");

            try
            {
                messageStream.Write(message);
            }
            catch (Exception ex)
            {
                Disconnect();
                Console.WriteLine(ex.Message);
                Console.WriteLine("ConnectionCheck - bad");
            }

            if (Name == "debug")
                Console.WriteLine("Exit ConnectionCheck");
        }



        private void ReceiveMessageActionResponse(NetworkMessage message)
        {
            try
            {
                MessageReceived?.Invoke(this, (Message)message.Obj);
                Console.WriteLine("Message received");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Error in Message received");
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
                messageStream.Write(request);

                ActionEnum requsetResult = messageStream.Read().Action;

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

        public IEnumerable<string> GetUsersActionRequest()
        {
            NetworkStreamMutex.WaitOne();
            IEnumerable<string> res = null;

            try
            {
                NetworkMessage request = new NetworkMessage(ActionEnum.get_users);
                messageStream.Write(request);
                NetworkMessage response = messageStream.Read();
                res = (List<string>)response.Obj;
                Console.WriteLine("GetUsers receive");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            NetworkStreamMutex.ReleaseMutex();
            return res;
        }

        public bool LoginActionRequest(string name)
        {
            NetworkStreamMutex.WaitOne();
            bool res = false;

            try
            {
                NetworkMessage request = new NetworkMessage(ActionEnum.login, name);
                messageStream.Write(request);

                ActionEnum response = messageStream.Read().Action;

                if (response == ActionEnum.ok)
                {
                    Name = name;
                    ConnectionState = ClientState.LoggedIn;
                    res = true;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            NetworkStreamMutex.ReleaseMutex();
            return res;
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
