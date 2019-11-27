using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace QChat.Server.DataManagment
{
    class DBConnectionProvider
    {
        private static DBConnectionProvider _singleton;
        public static DBConnectionProvider Instance
        {
            get
            {
                if (_singleton == null) _singleton = new DBConnectionProvider();
                return _singleton;
            }
        }

        public NpgsqlConnection Connection { get; private set; }

        private DBConnectionProvider()
        {
            ConnectToDB().Wait();
        }

        private async Task ConnectToDB()
        {
            var connectionString = "Host=localhost;Username=postgres;Password=passforroot;Database=QChatDB";
            Connection = new NpgsqlConnection(connectionString);
            await Connection.OpenAsync();

            if (Connection.State == System.Data.ConnectionState.Closed) Console.WriteLine("Connection to DB Failed");
        }
    }
}
