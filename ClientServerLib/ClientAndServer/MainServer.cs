using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using ClientServerLib.Common;
using ClientServerLib.Additional;

namespace ClientServerLib.ClientAndServer
{
    public class MainServer : ClientServerBase
    {
        Dictionary<int, ChatRoom> rooms = new Dictionary<int, ChatRoom>();
        
        TcpListener tcpListener;
        bool isWorking;

        public delegate void ServerInfoDelegate(string message);
        public event ServerInfoDelegate onServerInform;

        public MainServer(string ip, ushort port)
        {
            tcpListener = new TcpListener(IPAddress.Parse(ip), port);
        }

        ~MainServer()
        {
            Database.CloseConnection();
        }

        private string GetAllUsersInChatRoom(ChatRoom chatroom)
        {
            string names = "";
            int count = 0;
            foreach(var client in chatroom.ClientsInRoom)
            {
                if (!string.IsNullOrEmpty(client.UserLogin))
                {
                    names += client.UserLogin + "\n";
                    count++;
                }
            }
            names = names.Remove(names.Length - 1);
            names = $"Users in this chat ({count}):\n" + names;
            return names;
        }

        public async Task<bool> Start()
        {
            isWorking = true;
            try
            {
                if (!await Database.OpenConnection())
                {
                    throw new Exception("Ошибка соединения с сервером БД.");
                }
                //await GetAllRooms();
                //rooms.Add(1, new ChatRoom("General"));
                tcpListener.Start();
                StartAcceptingClients();
                return true;
            }
            catch(Exception ex)
            {
                onServerInform?.Invoke($"Ошибка запуска сервера: " + ex.Message);
                return false;
            }
        }

        public void Stop()
        {
            isWorking = false;
            tcpListener.Stop();
            rooms.Clear();
            onServerInform?.Invoke($"Сервер останавливает работу");
        }

        private async Task StartAcceptingClients()
        {
            while(isWorking)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                ClientObject clientObject = new ClientObject(client);
                
                //rooms[0].AddClientToRoom(clientObject);
                StartListeningClient(clientObject);
            }
        }

        private async Task StartListeningClient(ClientObject client)
        {
            try
            {
                TcpClient socket = client.Socket;
                while (socket.Connected)
                {
                    string message = await base.WaitMessage(socket);
                    HandleMessage(message, client);
                }
                socket.Close();
            }
            catch(Exception ex)
            {
                try
                {
                    RemoveClientFromRooms(client);
                    onServerInform?.Invoke($"{client.UserLogin} отключается");
                    if (!string.IsNullOrEmpty(client.UserLogin))
                        NotifyAllClients($"{client.UserLogin} disconnects.");
                    RemoveEmptyRooms();
                }
                catch (Exception exx) { onServerInform?.Invoke(exx.Message); }
            }
        }

        private void RemoveEmptyRooms()
        {
            for (int i = rooms.Count - 1; i >= 0; i --)
            {
                if (rooms.ElementAt(i).Value.ClientsInRoom.Count == 0)
                {
                    rooms.Remove(rooms.ElementAt(i).Key);
                }
            }
        }

        private void RemoveClientFromRooms(ClientObject client)
        {
            //foreach (ChatRoom r in client.ClientsChatRooms)
            for (int i= client.ClientsChatRooms.Count - 1; i>=0; i--)
            {
                client.ClientsChatRooms[i].RemoveClientFromRoom(client);
            }
        }

        //обработать входящее от клиента сообщение
        private async void HandleMessage(string message, ClientObject fromClient)
        {
            if (message.Length == 0)
                return;
            string decryptedMessage = await Crypto.Decrypt(message);
            if (ProcessMessageCommand(decryptedMessage, fromClient).Result)
                return;

            if (!decryptedMessage.StartsWith(ChatSyntax.ImageDiv))
            {
                onServerInform?.Invoke($"{fromClient.UserLogin}: {decryptedMessage}");
                onServerInform?.Invoke($"{fromClient.UserLogin}: {message}");
            }

            NotifyClients(decryptedMessage, fromClient);
            if (!await Database.AddMessage(decryptedMessage, fromClient, fromClient.ActiveChatRoom.Name))
                onServerInform($"Ошибка сохранения сообщения от клиента {fromClient.UserLogin} из комнаты {fromClient.ActiveChatRoom.Name} в базе.\nСообщение: {decryptedMessage}");
        }

        //отправить сообщение всем в чате клиента
        private async void NotifyClients(string message, ClientObject fromClient)
        {
            await NotifyClients(message, fromClient, fromClient.ActiveChatRoom.ClientsInRoom, fromClient.ActiveChatRoom);
        }

        //отправить сообщение определенному клиенту
        private async void NotifyClient(string message, ClientObject toClient)
        {
            await NotifyClients(message, null, new List<ClientObject> { toClient }, toClient.ActiveChatRoom);//rooms[1]);
        }

        //отправить сообщение всем клиентам
        private async void NotifyAllClients(string message)
        {
            await NotifyClients(message, null, rooms[1].ClientsInRoom, rooms[1]);
        }

        //отправить списку клиентов
        private async Task NotifyClients(string message, ClientObject fromClient, List<ClientObject> toClients, ChatRoom room)
        {
            if (!message.StartsWith("/"))
            {
                string fromClientName = fromClient == null ? "" : fromClient.UserLogin;
                message = String.Format("{0}{1}{2}{1}{3}", fromClientName, ChatSyntax.MessageDiv, message, room.Name);
            }

            string encryptedMessage = await Crypto.Encrypt(message);
            byte[] buffer = await base.GetMessageBytes(encryptedMessage);
            try
            {
                foreach (ClientObject client in toClients)
                {
                    if (client == null || string.IsNullOrEmpty(client.UserLogin))
                        continue;
                    await base.SendMessageToClient(client.Socket, buffer);
                }
            }
            catch(Exception ex) { onServerInform?.Invoke("Невозможно оповестить клиента: " + ex.Message); }
        }

        private async Task<bool> ProcessMessageCommand(String message, ClientObject client)
        {
            bool processed = false;
            if (message[0] == '/')
            {
                //Проверяем идет ли после начала команды пробел
                string command = message.Split(' ')[0];
                string[] command_arguments = new string[0];
                if (message.Length > command.Length)
                    command_arguments = message.Substring(command.Length + 1).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                //login
                if (command == ChatSyntax.LoginCmd)
                {
                    Login(client, command_arguments);
                }
                //room
                else if (command == ChatSyntax.CreateRoomCmd)
                {
                    CreateRoom(client, command_arguments[0]);
                }
                //help
                else if (command == ChatSyntax.HelpCmd)
                {
                    NotifyClient(ChatSyntax.HelpString, client);
                }
                //list
                else if (command == ChatSyntax.UserListCmd)
                {
                    NotifyClient(GetAllUsersInChatRoom(client.ActiveChatRoom), client);
                }
                //reg
                else if (command == ChatSyntax.RegCmd)
                {
                    Register(client, command_arguments);
                }
                //enterroom
                else if (command == ChatSyntax.EnterRoomCmd)
                {
                    EnterRoom(client, command_arguments[0]);
                }
                //invite
                else if (command == ChatSyntax.InviteToRoomCmd)
                {
                    InviteToRoom(client, command_arguments);
                }

                processed = true;
            }
            return processed;
        }

        //load history
        private async Task<bool> SendChatHistoryToClient(ClientObject client, List<ChatRoom> chatrooms)
        {
            foreach(ChatRoom cr in chatrooms)
            {
                string messages = await Database.GetMessagesFromRoom(cr.Name);
                if (!string.IsNullOrEmpty(messages))
                {
                    messages = ChatSyntax.ChatHistoryCmd + " " + cr.Name + "\n" + messages;
                    NotifyClient(messages, client);
                }
            }
            return true;
        }

        private ClientObject GetClientByLogin(string login)
        {
            foreach(var room in rooms.Values)
            {
                foreach(var client in room.ClientsInRoom)
                {
                    if (client.UserLogin == login)
                    {
                        return client;
                    }
                }
            }
            return null;
        }

        private string GetClientRoomsString(ClientObject client)
        {
            string roomsList = "";
            foreach(ChatRoom cr in client.ClientsChatRooms)
            {
                roomsList += cr.Name + "\n";
            }
            return roomsList;
        }

        private async Task GreetNewClient(ClientObject client, int id, string login)
        {
            client.Login(id, login);

            await Database.AddClientRoomsToCollection(client, rooms);
            client.SetActiveChatRoom(rooms[1]);
            NotifyClient(ChatSyntax.SignedInSignalCmd, client);
            NotifyClient($"{ChatSyntax.CreateRoomCmd} {GetClientRoomsString(client)}", client);
            bool done = await SendChatHistoryToClient(client, client.ClientsChatRooms);

            if (done)
                onServerInform?.Invoke($"{login} подключается");
            NotifyAllClients($"{client.UserLogin} connects.");
        }

        private async Task Login(ClientObject client, string[] loginAndPassword)
        {
            if (string.IsNullOrEmpty(client.UserLogin))
            {
                if (loginAndPassword.Length != 2)
                    return;
                int id = await Database.LoginUser(loginAndPassword[0], Crypto.GetSha256(loginAndPassword[1]));
                if (id >= 0)
                {
                    GreetNewClient(client, id, loginAndPassword[0]);
                }
            }
        }

        private async Task CreateRoom(ClientObject client, string roomName)
        {
            try
            {
                int roomId = await Database.AddRoom(roomName);
                if (roomId == -1)
                {
                    NotifyClient($"This room already exists!", client);
                    return;
                }
                if (!await Database.GiveInvite(client.UserLogin, roomName))
                {
                    onServerInform($"Ошибка выдачи приглашения пользователю {client.UserLogin} в комнату {roomName}.");
                    return;
                }
                ChatRoom newRoom = new ChatRoom(roomName);
                rooms.Add(roomId, newRoom);
                newRoom.AddClientToRoom(client);
                NotifyClient($"{ChatSyntax.CreateRoomCmd} {roomName}", client);
            }
            catch
            {
                NotifyClient($"Couldn't create room with this name.", client);
            }
        }

        private async Task Register(ClientObject client, string[] loginAndPassword)
        {
            if (!string.IsNullOrEmpty(client.UserLogin))
                return;
            if (loginAndPassword.Length != 2)
                return;
            int id = await Database.RegisterNewUser(loginAndPassword[0], Crypto.GetSha256(loginAndPassword[1]));
            if (id >= 0 && await Database.GiveInvite(loginAndPassword[0], "General"))
            {
                GreetNewClient(client, id, loginAndPassword[0]);
            }
        }

        private async Task EnterRoom(ClientObject client, string roomName)
        {
            try
            {
                ChatRoom newActiveRoom = GetUsersChatRoomByName(client, roomName);
                if (newActiveRoom == null)
                {
                    NotifyClient($"You don't have chat room with this name!", client);
                    return;
                }
                client.SetActiveChatRoom(newActiveRoom);
                NotifyClient($"{ChatSyntax.EnterRoomCmd} {roomName}", client);
            }
            catch
            {
                NotifyClient("Can't enter this room!", client);
            }
        }

        private ChatRoom GetUsersChatRoomByName(ClientObject client, string chatRoomName)
        {
            ChatRoom foundChatRoom = null;
            foreach (ChatRoom r in client.ClientsChatRooms)
            {
                if (r.Name == chatRoomName)
                {
                    foundChatRoom = r;
                    break;
                }
            }
            return foundChatRoom;
        }

        private async Task InviteToRoom(ClientObject client, string[] userAndRoomName)
        {
            try
            {
                if (userAndRoomName.Length < 2)
                    return;
                //if (!await Database.CheckInvite(client, userAndRoomName[1]))
                ChatRoom whereToInvite = GetUsersChatRoomByName(client, userAndRoomName[0]);
                if (whereToInvite == null)
                {
                    NotifyClient($"You don't have access to this room.", client);
                    return;
                }
                for (int i = 1; i < userAndRoomName.Length; i++)
                {
                    if (await Database.GiveInvite(userAndRoomName[i], whereToInvite.Name))
                    {
                        NotifyClient($"You've invited {userAndRoomName[i]} to the room {whereToInvite.Name}!", client);
                        var invitedClient = GetClientByLogin(userAndRoomName[i]);
                        if (invitedClient != null)
                        {
                            NotifyClient($"You've been invited to the room {whereToInvite.Name}!", invitedClient);
                            whereToInvite.AddClientToRoom(invitedClient);
                            SendChatHistoryToClient(invitedClient, new List<ChatRoom> { whereToInvite });
                            NotifyClient(ChatSyntax.CreateRoomCmd + " " + whereToInvite.Name, invitedClient);
                        }
                    }
                }
            }
            catch
            {
                NotifyClient($"Can't send invasion to this room.", client);
            }
        }
    }
}
