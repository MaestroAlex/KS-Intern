using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatConsoleServer
{
    class DBManager
    {
        private static DBManager Instance;
        private NpgsqlConnection connection;

        private Mutex _lockObject = new Mutex();

        private DBManager()
        {
            InitDataBaseConnectionAsync().Wait(2000);
        }

        private async Task InitDataBaseConnectionAsync()
        {
            var connectString = "Host=localhost;Username=postgres;Password=1548;Database=KSInternChatBD";
            connection = new NpgsqlConnection(connectString);
            await connection.OpenAsync();
        }

        public static DBManager GetInstance()
        {
            if (Instance == null)
            {
                Instance = new DBManager();
            }

            return Instance;
        }

        public async Task<bool> RegisterNewUser(string username, string password)
        {
            try
            {
                using (var cmd = new NpgsqlCommand($"INSERT INTO users VALUES ('{username}','{password}');", connection))
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

        public async Task<bool> CheckUserRegistration(string username)
        {
            try
            {
                using (var cmd = new NpgsqlCommand($"SELECT username FROM users;", connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (username.Equals(reader["username"]))
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

        public async Task<bool> AcceptUserLogin(string name, string password)
        {
            bool result = false;
            var SqlRequest = $"SELECT username,passw FROM users";
            try
            {
                using (var cmd = new NpgsqlCommand(SqlRequest, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (name.Equals(reader["username"]) && password.Equals(reader["passw"]))
                            {
                                result = true;
                            }
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

        public async Task<string> LoadChatHistory(int id)
        {
            string result = "";
            try
            {
                using (var cmd = new NpgsqlCommand($"SELECT message FROM messages WHERE chat_id={id};", connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result += (reader["message"].ToString()+'~');
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

        public async Task InsertUserToChat(string name, int id)
        {   
            try
            {
                using (var cmd = new NpgsqlCommand($"INSERT INTO users_by_chats VALUES('{name}','{id}');", connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        public async Task InsertNewMessage(int chatID, string sender, string message)
        {
            try
            {
                using (var cmd = new NpgsqlCommand($"INSERT INTO messages(sender, chat_id, message) VALUES('{sender}', {chatID}, '{message}');", connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<List<int>> GetUsersChats(string name)
        {

            List<int> result = new List<int>();
            try
            {
                using (var cmd = new NpgsqlCommand($"SELECT chat_id FROM users_by_chats WHERE username = '{name}';", connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var id = int.Parse(reader["chat_id"].ToString());
                            if (id > 1)
                            {
                                result.Add(id);
                            }
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

        public async Task<Dictionary<int,string>> GetChats()
        {
            Dictionary<int,string> result = new Dictionary<int, string>();

            try
            {
                using (var cmd = new NpgsqlCommand($"SELECT * FROM chats;", connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var id = int.Parse(reader["id"].ToString());
                            var name = reader["name"].ToString();
                            result.Add(id,name);
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

        public async Task<int> GetChatID(string member1, string member2)
        {
            int result = 0;
            try
            {
                using (var cmd = new NpgsqlCommand($"SELECT * FROM chats WHERE name = '{member1}:{member2}';", connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result = int.Parse(reader["id"].ToString());
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

        public async Task InsertNewChat(string name,int id)
        {
            try
            {
                using (var cmd = new NpgsqlCommand($"INSERT INTO chats VALUES({id},'{name}');", connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }


}
