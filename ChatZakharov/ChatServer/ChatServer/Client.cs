using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransitPackage;

namespace ChatServer
{
    class Client
    {
        Server server;
        public TcpClient TcpClient { get; private set; }
        NetworkStream networkStream;
        NetworkMessageStream messageStream;
        bool streamIsOpen;

        public string Name { get; private set; }
        public DataQueue ActionQueue { get; private set; }

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
                        NetworkMessage request = messageStream.Read();
                        Process(request);
                    }
                    else if (ActionQueue.Messages.Count != 0)
                    {
                        NetworkMessage request = new NetworkMessage(ActionEnum.send_message);
                        Process(request);
                    }
                    else if (ActionQueue.NewUsers.Count != 0)
                    {
                        NetworkMessage request = new NetworkMessage(ActionEnum.user_logged_in);
                        Process(request);
                    }
                    else if (ActionQueue.DeletedUsers.Count != 0)
                    {
                        NetworkMessage request = new NetworkMessage(ActionEnum.user_logged_out);
                        Process(request);
                    }
                    else if (ActionQueue.ConnectionCheckRequired)
                    {
                        NetworkMessage request = new NetworkMessage(ActionEnum.connection_check);
                        Process(request);
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
                    case ActionEnum.get_users:
                        GetUsersActionResonse(message);
                        break;
                    case ActionEnum.user_logged_in:
                        UserLoggedInNotificationActionResponse(message);
                        break;
                    case ActionEnum.user_logged_out:
                        UserLoggedOutNotificationActionResponse(message);
                        break;
                    case ActionEnum.connection_check:
                        ConnectionCheckActionRequest(message);
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

        private void UserLoggedOutNotificationActionResponse(NetworkMessage request)
        {
            try
            {
                request.Obj = ActionQueue.DeletedUsers.Dequeue();
                messageStream.Write(request);
            }
            catch (Exception e)
            {
                Console.WriteLine("UserLoggedOutNotification exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
                LogoutActionResponse();
            }
        }

        private void UserLoggedInNotificationActionResponse(NetworkMessage request)
        {
            try
            {
                request.Obj = ActionQueue.NewUsers.Dequeue();
                messageStream.Write(request);
            }
            catch (Exception e)
            {
                Console.WriteLine("UserLoggedInNotification exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
                LogoutActionResponse();
            }
        }

        private void GetUsersActionResonse(NetworkMessage message)
        {
            try
            {
                List<string> users = server.GetUserList(Name);
                NetworkMessage response = new NetworkMessage(ActionEnum.ok, users);
                messageStream.Write(response);

                Console.WriteLine("GetUsers send to {0} ({1})",
                    Name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("GetUsersActionResonse exception - {0}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());
                Console.WriteLine(e.Message);
            }
        }

        private void ConnectionCheckActionRequest(NetworkMessage message)
        {
            //if (Name == "debug")
            //    Console.WriteLine("Enter connectionCheck");


            try
            {
                messageStream.Write(message);
                messageStream.Read();
                ActionQueue.ConnectionCheckRequired = false;
            }
            catch (Exception e)
            {
                Console.WriteLine("ConnectionCheck exception - {0}({1})",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(), Name);
                Console.WriteLine(e.Message);
                LogoutActionResponse();
            }

            //if (Name == "debug")
            //    Console.WriteLine("Exit connectionCheck");
        }

        private void SendMessageActionResponse(NetworkMessage request)
        {
            //if (Name == "debug")
            //    Console.WriteLine("Enter SendMessage");

            try
            {
                Message cur = ActionQueue.Messages.Dequeue();
                NetworkMessage message = new NetworkMessage(ActionEnum.receive_message, cur);
                messageStream.Write(message);

                Console.WriteLine("Message \"{0}\" send from {1} to {2}",
                    cur.Text, cur.From, cur.To);

            }
            catch (Exception e)
            {
                Console.WriteLine("Send message exception - {0} ({1}",
                    ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString(), e.Message);
            }

            //if (Name == "debug")
            //    Console.WriteLine("Exit SendMessage");
        }

        private void ReceiveMessageActionResponse(NetworkMessage message)
        {
            try
            {
                Console.WriteLine("Message received from {0}", Name);

                bool res = server.SendToclient((Message)message.Obj);
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

        private void LoginActionResponse(NetworkMessage message)
        {
            try
            {
                Console.WriteLine("User {0} try to login ",
                    ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString());

                string name = message.Obj.ToString();
                bool check = !server.UserExist(name);

                if (!check)
                {
                    Console.WriteLine("User {0} from {1} already exist ",
                        name, ((IPEndPoint)TcpClient.Client?.RemoteEndPoint).Address.ToString());

                    NetworkMessage response = new NetworkMessage(ActionEnum.bad);
                    messageStream.Write(response);
                }
                else
                {
                    Console.WriteLine("User {0} from {1} Logged in ",
                        name, ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.ToString());

                    Name = name;
                    NetworkMessage response = new NetworkMessage(ActionEnum.ok);
                    messageStream.Write(response);
                    server.UserLoggedInNotify(name);
                }
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
                server.UserLoggedOutNotify(Name);

                Console.WriteLine("User {0} from {1} Logged out and disconnected ",
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
    }
}
