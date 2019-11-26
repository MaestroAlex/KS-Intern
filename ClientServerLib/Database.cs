using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace ClientServerLib
{
    class Database
    {
        public static async Task<bool> RegisterNewUser(string login, string password)
        {
            string connectionString = "Server=localhost; Port=5432; User Id=postgres; Password=12345; Database=chatdb";
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    NpgsqlCommand command = new NpgsqlCommand($"INSERT INTO users (login, password) VALUES ('{login}', '{password}')", connection);
                    using (NpgsqlDataReader dataReader = await command.ExecuteReaderAsync()) { }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> LoginUser(string login, string password)
        {
            string connectionString = "Server=localhost; Port=5432; User Id=postgres; Password=12345; Database=chatdb";
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    NpgsqlCommand command = new NpgsqlCommand($"SELECT id FROM users WHERE login LIKE '{login}' AND password LIKE '{password}'", connection);
                    using (NpgsqlDataReader dataReader = await command.ExecuteReaderAsync())
                    {
                        return dataReader.Read();
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> AddRoom(string roomName)
        {
            string connectionString = "Server=localhost; Port=5432; User Id=postgres; Password=12345; Database=chatdb";
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    NpgsqlCommand command = new NpgsqlCommand($"INSERT INTO room (name) VALUES ('{roomName}')", connection);
                    using (NpgsqlDataReader dataReader = await command.ExecuteReaderAsync()) { }

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> AddMessage(string message, ClientObject client)
        {
            string connectionString = "Server=localhost; Port=5432; User Id=postgres; Password=12345; Database=chatdb";
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    NpgsqlCommand command = new NpgsqlCommand($"INSERT INTO message (user_id, room_id, message) VALUES " +
                        $"( (SELECT id FROM users WHERE login LIKE '{client.UserLogin}'), " +
                        $" (SELECT id FROM room WHERE name LIKE '{client.ChatRoom.Name}'), '{message}');", connection);
                    using (NpgsqlDataReader dataReader = await command.ExecuteReaderAsync()) { }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<List<string>> GetAllRooms()
        {
            string connectionString = "Server=localhost; Port=5432; User Id=postgres; Password=12345; Database=chatdb";
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    NpgsqlCommand command = new NpgsqlCommand($"SELECT name FROM room", connection);
                    List<string> roomNames = new List<string>();
                    using (NpgsqlDataReader dataReader = await command.ExecuteReaderAsync())
                    {
                        while(dataReader.Read())
                        {
                            roomNames.Add(dataReader[0].ToString());
                        }
                    }
                    return roomNames;
                }
            }
            catch
            {
                return null;
            }
        }

        public static async Task<bool> CheckInvite(ClientObject client, string roomName)
        {
            string connectionString = "Server=localhost; Port=5432; User Id=postgres; Password=12345; Database=chatdb";
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    NpgsqlCommand command = new NpgsqlCommand($"SELECT user_id FROM user_in_room WHERE user_id = (SELECT id FROM users WHERE login LIKE '{client.UserLogin}') AND " +
                        $"room_id = (SELECT id FROM room WHERE name LIKE '{roomName}')", connection);
                    List<string> roomNames = new List<string>();
                    using (NpgsqlDataReader dataReader = await command.ExecuteReaderAsync())
                    { return dataReader.Read(); }
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> GiveInvite(string clientName, string roomName)
        {
            string connectionString = "Server=localhost; Port=5432; User Id=postgres; Password=12345; Database=chatdb";
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    NpgsqlCommand command = new NpgsqlCommand($"INSERT INTO user_in_room (user_id, room_id) VALUES " +
                            $"((SELECT id FROM users WHERE login LIKE '{clientName}'), " +
                            $"(SELECT id FROM room WHERE name LIKE '{roomName}'))", connection);
                    using (NpgsqlDataReader dataReader = await command.ExecuteReaderAsync()) { }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
