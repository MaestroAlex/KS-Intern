using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitPackage;

namespace ChatClient.Models
{
    public class MainModel
    {
        public static ObservableCollection<Channel> ConnectedChannels { get; private set; }

        public static Client Client { get; private set; }

        static MainModel()
        {
            Client = new Client(Config.GetConfig());
        }

        public static async Task GetChannels()
        {
            ConnectedChannels = new ObservableCollection<Channel>(await Client.GetChannelsActionRequest());
        }
    }
}
