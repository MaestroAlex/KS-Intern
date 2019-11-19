using NetworkLibrary.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLibrary
{

    public class MainServer : NetworkCore
    {
        private TcpListener _tcpListener;

        private List<ClientEssence> _clients = new List<ClientEssence>();
        private Dictionary<int, ChatEssence> _chats = new Dictionary<int, ChatEssence>();

        private bool _shouldWork;

        public MainServer(string ip, ushort port)
        {
            _tcpListener = new TcpListener(IPAddress.Parse(ip), port); 

            this.MessageEvent += this.InterceptMessage;
        }

        public async Task<bool> StartServer()
        {
            var dataChats = await DataBaseManager.Instance().GetChatList();

            foreach(var tup in dataChats)
            {
                this._chats.Add(tup.Item1, new ChatEssence(tup.Item2));
            }

            bool res = false;
            try
            {
                _tcpListener.Start();
                StartAcceptClients();
                res = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            return res;
        }

        private async Task ListenToClient(ClientEssence cl)
        {
            await base.ListenToClient(cl);
            _clients.Remove(cl);
        }

        private async void StartAcceptClients()
        {
            _shouldWork = true;
            while (_shouldWork)
            {
                var client = new ClientEssence(await _tcpListener.AcceptTcpClientAsync());
                this._clients.Add(client);
                this._chats.First().Value.AddClientToChat(client);
                this.ListenToClient(client);
            }
            _clients.Clear();
            _tcpListener.Stop();
        }

        private async void InterceptMessage(string message, object sender)
        {
            var client = sender as ClientEssence;
            Console.WriteLine($"{client.UserLogin} : {message}");
            if (client.UserId == 0 && !message.StartsWith(CommonData.LoginMessage))
            {
                Console.WriteLine("Unregistered user message : " + message);
                return;
            }
            if (message.StartsWith(CommonData.LoginMessage))
            {
                var login = message.Substring(2).Split(CommonData.CommonDataSeparator);
                if (login.Length != 2)
                {
                    Console.WriteLine("Uncorrect login message");
                    return;
                }
                else
                {
                    var exist = await DataBaseManager.Instance().CheckUserRegistration(login[0]);
                    bool logged = false;
                    int[] usChats = { };
                    if (exist)
                    {
                        logged = await DataBaseManager.Instance().DoUserLogin(login[0], login[1]);

                        usChats = await DataBaseManager.Instance().GetUserChats(login[0]);
                        foreach(var id in usChats)
                        {
                            _chats.First(i => i.Key == id).Value.AddClientToChat(client);
                        }
                    }
                    else
                    {
                        var loggedIn = await DataBaseManager.Instance().DoUserRegistration(login[0], login[1]);
                        await DataBaseManager.Instance().AddUserToChat(login[0], _chats.First().Key);

                        logged = loggedIn;
                    }

                    if(logged)
                    {
                        client.DoLogin(login[0]);
                        await SendChatList(client);
                        foreach(var id in usChats)
                        {
                            var msgHistory = await DataBaseManager.Instance().GetMesssageHistoryForUser(id);
                            foreach(var m in msgHistory)
                            {
                                await WriteToClient(client, m.Item1.Insert(3, m.Item2 + '|'));
                            }
                        }
                        
                    }
                    else
                    {
                        WriteToClient(client, CommonData.LoginMessage+"Incorrect pass");
                    }
                }
            }
            else if(message.StartsWith(CommonData.GetChatLIst))
            {
                await SendChatList(client);
            }
            else if(message.StartsWith(CommonData.GetUsersList))
            {
                string userList = CommonData.GetUsersList;
                foreach(var cl in _clients)
                {
                    userList += cl.UserLogin + CommonData.CommonDataSeparator;
                }
                this.WriteToClient(client, userList);
            }
            else if(message.StartsWith(CommonData.CreateChatMessage))
            {
                var args = message.Substring(2).Split(CommonData.CommonDataSeparator);

                var chatId = await DataBaseManager.Instance().RegisterChat(args[0]);

                var chat = new ChatEssence(args[0]);
                chat.AddClientToChat(client);
                foreach(var clientName in args)
                {
                    try
                    {
                        chat.AddClientToChat(this._clients.First(i => i.UserLogin == clientName));
                        await DataBaseManager.Instance().AddUserToChat(clientName, chatId);
                    }
                    catch { }
                }
                this._chats.Add(chatId, chat);
                WriteToClientsInChat(CommonData.UpdateStateMessage, chat);
            }
            else
            {
                if (message.Length > 2)
                {
                    int chatId;
                    if(int.TryParse(message.Substring(2, 1), out chatId))
                    {
                        ChatEssence chat;
                        if (this._chats.TryGetValue(chatId, out chat))
                        {
                            await DataBaseManager.Instance().RegisterChatMessage(message, chatId, client.UserLogin);
                            WriteToClientsInChat(message.Insert(3, client.UserLogin + '|'), chat);
                        }
                    }
                }
                else
                {
                    WriteToAllClients($"Message from {client.UserLogin}: " + message);
                }
            }
        }

        private async Task SendChatList(ClientEssence client)
        {
            string chatlist = CommonData.GetChatLIst;
            var lk = await DataBaseManager.Instance().GetUserChats(client.UserLogin);
            foreach (var l in lk)
            {
                if (_chats[l].Users.Contains(client))
                    chatlist += _chats[l].ChatName + CommonData.CommonDataSeparator + l + CommonData.CommonDataSeparator;
            }
            
            await WriteToClient(client, chatlist);
        }

        private async Task WriteToClientsInChat(string message, ChatEssence chat)
        {
            foreach(var cl in chat.Users)
            {
                WriteToClient(cl, message);
            }
        }

        private async Task WriteToAllClients(string message, ClientEssence exceptClient = null)
		{
			foreach(var cl in _clients)
			{
                if(cl != exceptClient)
				    WriteToClient(cl, message);
			}
		}
    }
}
