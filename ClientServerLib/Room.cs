using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerLib
{
    public class Room
    {
        string name;
        List<ClientObject> clients;
        public List<ClientObject> ClientsInRoom { get { return clients; } } 
        public string Name { get { return name; } }

        public Room(string name)
        {
            clients = new List<ClientObject>();
            this.name = name;
        }

        public void AddClientToRoom(ClientObject newClient)
        {
            clients.Add(newClient);
            newClient.ChatRoom = this;
        }

        public void RemoveClientFromRoom(ClientObject client)
        {
            clients.Remove(client);
            client.ChatRoom = null;
        }
    }
}
