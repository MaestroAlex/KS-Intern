using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace QChat.Server.DataManagment
{
    class GroupDataManager
    {
        private NpgsqlConnection _connection;

        public GroupDataManager()
        {
            _connection = DBConnectionProvider.Instance.Connection;
        }

        public async Task<int> RegisterGroupAsync(string name)
        {
            try
            {
                using (var command = new NpgsqlCommand($"INSERT INTO Groups(Id, Name) VALUES ({name.GetHashCode()},'{name}')", _connection))
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
        public int RegisterGroup(string name)
        {
            try
            {
                using (var command = new NpgsqlCommand($"INSERT INTO Groups(Name) VALUES ('{name}')", _connection))
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

        public async Task<bool> GroupRegisteredAsync(string name)
        {
            try
            {
                using (var command = new NpgsqlCommand($"SELECT Name FROM Groups"))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while(await reader.ReadAsync())
                        {
                            if (name.Equals(reader.GetString(0)))
                                return true;
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }
        public bool GroupRegistered(string name)
        {
            try
            {
                using (var command = new NpgsqlCommand($"SELECT Name FROM Groups"))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (name.Equals(reader.GetString(0)))
                                return true;
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        public async Task<string> GetGroupNameAsync(int id)
        {
            try
            {
                using (var command = new NpgsqlCommand($"SELECT Name FROM Groups WHERE Id ='{id}'", _connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                            return null;
                        else
                            return reader.GetString(0);
                    }
                }
            }
            catch
            {

            }
            return null;
        }
        public string GetGroupName(int id)
        {
            try
            {
                using (var command = new NpgsqlCommand($"SELECT Name FROM Groups WHERE Id ='{id}'", _connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;
                        else
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
