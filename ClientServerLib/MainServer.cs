using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace ClientServerLib
{
    public class MainServer
    {
        List<Room> rooms;
        TcpListener tcpListener;
        bool isWorking;

        public delegate void ServerInfoDelegate(string message);
        public event ServerInfoDelegate onServerInform;

        public MainServer(string ip, ushort port)
        {
            tcpListener = new TcpListener(IPAddress.Parse(ip), port);
        }

        private string AllUsers
        {
            get
            {
                int count = 0;
                if (rooms.Count == 0)
                    return "No rooms detected!";
                string names = "";
                foreach(Room r in rooms)
                {
                    foreach(ClientObject client in r.ClientsInRoom)
                    {
                        if (!string.IsNullOrEmpty(client.UserLogin))
                        {
                            names += client.UserLogin + "\n";
                            count++;
                        }
                    }
                }
                if (count == 0)
                    return "No clients in chat!";
                names = names.Remove(names.Length - 1);
                names = $"Clients in the chat ({count}):\n" + names;
                return names;
            }
        }

        public async Task<bool> Start()
        {
            isWorking = true;
            try
            {
                Debug.WriteLine("Сервер начинает работу");
                tcpListener.Start();
                await GetAllRooms();
                StartAcceptingClients();
                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return false;
            }
        }

        private async Task GetAllRooms()
        {
            rooms = new List<Room>();
            List<String> roomNames = await Database.GetAllRooms();
            if (roomNames == null)
                throw new Exception("Error trying get rooms from database!");
            foreach (string roomName in roomNames)
            {
                rooms.Add(new Room(roomName));
            }
        }

        public void Stop()
        {
            Debug.WriteLine("Сервер останавливает работу");
            isWorking = false;
            tcpListener.Stop();
            rooms.Clear();
        }

        private async Task StartAcceptingClients()
        {
            while(isWorking)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                ClientObject clientObject = new ClientObject(client);
                //clients.Add(clientObject);
                if (rooms.Count == 0)
                {
                    Database.AddRoom("General");
                    GetAllRooms();
                }
                rooms[0].AddClientToRoom(clientObject);
                onServerInform?.Invoke("Новый клиент соединяется");
                StartListeningClient(clientObject);
            }
            Debug.WriteLine("Сервер остановился");
        }

        private async Task StartListeningClient(ClientObject client)
        {
            try
            {
                TcpClient socket = client.Socket;
                while (socket.Connected)
                {
                    byte[] buffer = new byte[4];
                    NetworkStream dataStream = socket.GetStream();
                    await dataStream.ReadAsync(buffer, 0, 4);
                    int length = BitConverter.ToInt32(buffer, 0);
                    buffer = new byte[length];
                    dataStream.Read(buffer, 0, length);
                    string message = Encoding.UTF8.GetString(buffer);
                    Debug.WriteLine("Сообщение получено");
                    HandleMessage(message, client);
                }
                socket.Close();
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
            client.ChatRoom.RemoveClientFromRoom(client);
            onServerInform?.Invoke($"{client.UserLogin} отключается");

            if (!string.IsNullOrEmpty(client.UserLogin))
                NotifyAllClients($"{client.UserLogin} disconnects.", null);
        }

        //обработать входящее от клиента сообщение
        private async void HandleMessage(string message, ClientObject fromClient)
        {
            if (message.Length == 0)
                return;
            if (ProcessMessageCommand(message, fromClient).Result)
                return;

            if (!await Database.AddMessage(message, fromClient))
                onServerInform($"Ошибка сохранения сообщения от клиента {fromClient.UserLogin} из комнаты {fromClient.ChatRoom.Name} в базе.\nСообщение: {message}");
            NotifyClients(message, fromClient);
        }

        //отправить сообщение всем в чате клиента
        private void NotifyClients(string message, ClientObject fromClient)
        {
            NotifyClients(message, fromClient, fromClient.ChatRoom.ClientsInRoom);
        }

        //отправить сообщение определенному клиенту
        //удалить fromClient скорее всего.
        private void NotifyClient(string message, ClientObject fromClient, ClientObject toClient)
        {
            NotifyClients(message, fromClient, new List<ClientObject> {toClient });
        }

        //отправить сообщение всем клиентам
        //тоже удалить fromClient
        private void NotifyAllClients(string message, ClientObject fromClient)
        {
            List<ClientObject> clients = new List<ClientObject>();
            foreach(Room r in rooms)
            {
                clients.AddRange(r.ClientsInRoom);
            }

            NotifyClients(message, fromClient, clients);
        }

        //отправить списку клиентов
        private void NotifyClients(string message, ClientObject fromClient, List<ClientObject> toClients)
        {
            string clientPrefix = fromClient == null ? "" : $"{fromClient.UserLogin}: ";
            message = clientPrefix + message;

            byte[] buffer = GetMessageBytes(message);
            foreach (ClientObject client in toClients)
            {
                SendMessageToClient(client, buffer, message.Length + 4);
            }
        }

        private void SendMessageToClient(ClientObject client, byte[] buffer, int totalLength)//string message)
        {
            if (client == null || string.IsNullOrEmpty(client.UserLogin))
                return;
            TcpClient socket = client.Socket;
            NetworkStream dataStream = socket.GetStream();
            dataStream.Write(buffer, 0, totalLength);
            Debug.WriteLine("Сообщение отправлено");
        }

        private byte[] GetMessageBytes(string message)
        {
            byte[] buffer = new byte[message.Length + 4];
            byte[] messageLength = BitConverter.GetBytes(message.Length);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            buffer = messageLength.Concat(messageBytes).ToArray();
            return buffer;
        }

        private async Task<bool> ProcessMessageCommand(String message, ClientObject client)
        {
            bool processed = false;
            if (message[0] == '/')
            {
                int i = 0;
                foreach (string command in ChatSyntax.MessageCommands)
                {
                    if (message.StartsWith(command))
                    {
                        string command_argument = "";
                        if (message.Length > command.Length)
                        {
                            if (message[command.Length] != ' ')
                                return false;
                            else
                            {
                                command_argument = message.Substring(command.Length + 1);
                            }
                        }

                        //login
                        if (i == 0)
                        {
                            if (string.IsNullOrEmpty(client.UserLogin))
                            {
                                string[] log_pass = command_argument.Split(' ');
                                if (log_pass.Length < 2)
                                    return true;
                                if (await Database.LoginUser(log_pass[0], log_pass[1]))
                                {
                                    GreetNewClient(client, log_pass[0]);
                                }
                            }
                        }
                        //room
                        else if (i == 1)
                        {
                            if (rooms.FindIndex(r => r.Name == command_argument) == -1)
                            {
                                rooms.Add(new Room(command_argument));
                                if (!await Database.AddRoom(command_argument) || !await Database.GiveInvite(client.UserLogin, command_argument))
                                {
                                    onServerInform($"Ошибка сохранения комнаты {command_argument} в базе.");
                                    return true;
                                }
                                NotifyAllClients($"{command} {command_argument}", null);
                            }
                            else
                            {
                                NotifyClient($"This room already exists!", null, client);
                            }
                        }
                        //help
                        else if (i == 2)
                        {
                            NotifyClient(ChatSyntax.HelpString, null, client);
                        }
                        //list
                        else if (i == 3)
                        {
                            NotifyClient(AllUsers, null, client);
                        }
                        //reg
                        else if (i == 5)
                        {
                            if (!string.IsNullOrEmpty(client.UserLogin))
                                return true;
                            string[] log_pass = command_argument.Split(' ');
                            if (log_pass.Length < 2)
                                return true;
                            if (await Database.RegisterNewUser(log_pass[0], log_pass[1]) && await Database.GiveInvite(log_pass[0], "General"))
                            {
                                GreetNewClient(client, log_pass[0]);
                            }
                        }
                        //enter
                        else if (i == 6)
                        {
                            try
                            {
                                Room newRoom = rooms.Find(r => r.Name == command_argument);
                                if (await Database.CheckInvite(client, command_argument))
                                {
                                    client.ChatRoom.RemoveClientFromRoom(client);
                                    newRoom.AddClientToRoom(client);
                                    NotifyClient($"You entered the {newRoom.Name} chat room!", null, client);
                                }
                                else
                                {
                                    NotifyClient("You have no permission to enter this room!", null, client);
                                }
                            }
                            catch (ArgumentNullException ae)
                            {
                                NotifyClient("No such room name!", null, client);
                            }
                        }
                        //invite
                        else if (i == 7)
                        {
                            string[] roomInfo = command_argument.Split(' ');
                            if (roomInfo.Length < 2)
                                return true;
                            if (await Database.CheckInvite(client, roomInfo[1]) && await Database.GiveInvite(roomInfo[0], roomInfo[1]))
                            {
                                NotifyClient($"You've sent invation in room {roomInfo[1]} to {roomInfo[0]}!", null, client);
                                var invitedClient = GetClientByLogin(roomInfo[0]);
                                if (invitedClient != null)
                                {
                                    NotifyClient($"You've been invited to the room {roomInfo[1]}!", null, invitedClient);
                                }
                            }
                            else
                            {
                                NotifyClient($"Can't send invation!", null, client);
                            }
                        }

                        processed = true;
                        break;
                    }
                    i++;
                }
            }
            return processed;
        }

        private ClientObject GetClientByLogin(string login)
        {
            ClientObject search = null;
            foreach(var room in rooms)
            {
                foreach(var client in room.ClientsInRoom)
                {
                    if (client.UserLogin == login)
                    {
                        search = client;
                        break;
                    }
                }
            }
            return search;
        }

        private string GetRoomsList()
        {
            string roomsList = "";
            foreach(Room r in rooms)
            {
                roomsList += r.Name + "\n";
            }
            return roomsList.Remove(roomsList.Length - 1);
        }

        private void GreetNewClient(ClientObject client, string login)
        {
            client.SetLogin(login);
            NotifyClient("/signed_in", null, client);
            NotifyAllClients($"{client.UserLogin} connects.", null);
            NotifyClient($"{ChatSyntax.MessageCommands[1]} {GetRoomsList()}", null, client);
        }
    }
}
