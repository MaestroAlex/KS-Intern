using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace QChat.Server.DataManagment
{
    class RoomDataManager
    {
        private NpgsqlConnection _connection;

        public RoomDataManager()
        {
            _connection = DBConnectionProvider.Instance.Connection;
        }


        public int RegisterRoom(string name)
        {
            try
            {
                using (var command = new NpgsqlCommand($"INSERT INTO Rooms(Id, Name) VALUES ({name.GetHashCode()},'{name}')", _connection))
                {
                    command.ExecuteNonQuery();
                }

                return name.GetHashCode();                
            }
            catch
            {

            }
            return -1;
        }
        public async Task<int> RegisterRoomAsync(string name)
        {
            try
            {
                using (var command = new NpgsqlCommand($"INSERT INTO Rooms(Id, Name) VALUES ({name.GetHashCode()},'{name}')", _connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                return name.GetHashCode();
            }
            catch
            {

            }
            return -1;
        }

        public bool RoomRegistered(string name)
        {
            try
            {
                using (var command = new NpgsqlCommand($"SELECT Name FROM Groups", _connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            if (name.Equals(reader.GetString(0))) return true;
                    }
                }
            }
            catch
            {

            }
            return false;
        }
        public async Task<bool> RoomRegisteredAsync(string name)
        {
            try
            {
                using (var command = new NpgsqlCommand($"SELECT Name FROM Groups", _connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            if (name.Equals(reader.GetString(0)))
                                return true;
                    }
                }
            }
            catch
            {

            }
            return false;
        }

        public string GetRoomName(int id)
        {
            try
            {
                using (var command = new NpgsqlCommand($"SELECT Name FROM Rooms WHERE Id={id}", _connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) return null;
                        return reader.GetString(0);
                    }
                }
            }
            catch
            {

            }

            return null;
        }
        public async Task<string> GetRoomIdAsync(int id)
        {
            try
            {
                using (var command = new NpgsqlCommand($"SELECT Name FROM Rooms WHERE Id={id}", _connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync()) return null;
                        return reader.GetString(0);
                    }
                }
            }
            catch
            {

            }

            return null;
        }
    }
}
