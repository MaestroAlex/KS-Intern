using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Models
{
    public class MainModel
    {
        public static ObservableCollection<string> ConnectedUsers { get; private set; }

        public static Client Client { get; private set; }

        static MainModel()
        {
            Client = new Client(Config.GetConfig());
        }

        public static void GetUsers()
        {
            ConnectedUsers = new ObservableCollection<string>(Client.GetUsersActionRequest());
        }
    }
}
