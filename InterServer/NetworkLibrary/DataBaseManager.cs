using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkLibrary
{
    public class DataBaseManager
    {
        private static DataBaseManager instance;

        private NpgsqlConnection connection;
        private Mutex _lockObject = new Mutex();
        private DataBaseManager()
        {
            InitDataBaseConnectionAsync().Wait(2000);
        }

        private async Task InitDataBaseConnectionAsync()
        {
            var connectString = "Host=localhost;Username=postgres;Password=root;Database=postgres";
            connection = new NpgsqlConnection(connectString);
            await connection.OpenAsync();
        }

        private async Task<int> GetUserID(string userName)
        {
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT id FROM users WHERE name='" + userName + "';", connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            return reader.GetInt32(0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return -1;
        }

        public static DataBaseManager Instance()
        {
            if (instance == null)
                instance = new DataBaseManager();

            return instance;
        }

        public async Task<bool> CheckUserRegistration(string username)
        {
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT name FROM users", connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (username.Equals(reader["name"]))
                                return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return false;
        }

        public async Task<bool> DoUserRegistration(string username, string pass)
        {
            try
            {
                using (var cmd = new NpgsqlCommand("INSERT INTO users(name,nick) VALUES ('" + username + "','" + pass + "')", connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return false;
        }

        public async Task<bool> DoUserLogin(string v1, string v2)
        {
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT name,nick FROM users", connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (v1.Equals(reader["name"]) && v2.Equals(reader["nick"]))
                                return true;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            return false;
        }

        public async Task<int> RegisterChat(string chatName)
        {
            try
            {
                using(var cmd = new NpgsqlCommand("INSERT INTO chats(name) VALUES('" + chatName + "') RETURNING id", connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Console.WriteLine("Created chat with id:" + reader["id"]);
                            return reader.GetInt32(0);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            return -1;
        }

        public async Task AddUserToChat(string userName, int chatId)
        {
            try
            {
                int userId = await this.GetUserID(userName);
                using (var cmd = new NpgsqlCommand("INSERT INTO users_by_chats VALUES (" + chatId + "," + userId + ");", connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task RegisterChatMessage(string message, int roomId, string userName)
        {
            try
            {
                int userId = await this.GetUserID(userName);
                using (var cmd = new NpgsqlCommand("INSERT INTO messages(chat_id,user_id,message) VALUES ("
                 + roomId + ","
                 + userId + ",'"
                 + message + "');", connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<List<(int, string)>> GetChatList()
        {
            List<(int, string)> result = new List<(int, string)>();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT id,name FROM chats", connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add((reader.GetInt32(0), reader["name"] as string));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result;
        }

        public async Task<int[]> GetUserChats(string userName)
        {
            List<int> result = new List<int>();
            try
            {
                _lockObject.WaitOne(1000);
                Task.Run(async () =>
                {
                    try
                    {
                        int userId = await this.GetUserID(userName);
                        using (var cmd = new NpgsqlCommand("SELECT chat_id FROM users_by_chats WHERE user_id= " + userId
                            , connection))
                        {
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    result.Add(reader.GetInt32(0));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }).Wait();
                _lockObject.ReleaseMutex();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                _lockObject.ReleaseMutex();
            }
            return result.ToArray();
        }

        public async Task<(string, string)[]> GetMesssageHistoryForUser(int chatId)
        {
            List<(string, string)> result = new List<(string, string)>();
            try
            {
                using (var cmd = new NpgsqlCommand("select messages.message, users.name from messages " + 
"join users on users.id = messages.user_id where messages.chat_id=" + chatId
                    , connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add((reader.GetString(0),reader.GetString(1)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result.ToArray();
        }
    }
}
