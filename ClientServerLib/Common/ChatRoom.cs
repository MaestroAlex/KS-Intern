using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerLib.Common
{
    public class ChatRoom
    {
        string name;
        List<ClientObject> clients;
        public List<ClientObject> ClientsInRoom { get { return clients; } } 
        public string Name { get { return name; } }

        public ChatRoom(string name)
        {
            this.name = name;
            clients = new List<ClientObject>();
        }

        public void AddClientToRoom(ClientObject newClient)
        {
            clients.Add(newClient);
            newClient.ClientsChatRooms.Add(this);
        }

        public void RemoveClientFromRoom(ClientObject client)
        {
            clients.Remove(client);
            client.ClientsChatRooms.Remove(this);
        }
    }
}
