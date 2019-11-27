using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using QChat.Common;

namespace QChat.Server.DataManagment
{
    class UserDataManager
    {
        private NpgsqlConnection _connection;

        public UserDataManager()
        {
            _connection = DBConnectionProvider.Instance.Connection;
        }

        public async Task<bool> RegisterUserAsync(string login, int password)
        {
            try
            {
                using (var command = new NpgsqlCommand($"INSERT INTO users(id, login, password) VALUES ('{login.GetHashCode()}','{login}','{password}')", _connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return false;
        }
        public bool RegisterUser(string login, int password)
        {
            try
            {
                using (var command = new NpgsqlCommand($"INSERT INTO users(id, login, password) VALUES ('{login.GetHashCode()}','{login}','{password}')", _connection))
                {
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch
            {

            }

            return false;
        }

        public async Task<bool> UserRegisteredAsync(string login)
        {
            try
            {
                using (var command = new NpgsqlCommand("SELECT login FROM users", _connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            if (login.Equals(reader["login"]))
                                return true;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            return false;
        }
        public bool UserRegistered(string login)
        {
            try
            {
                using (var command = new NpgsqlCommand("SELECT login FROM user", _connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            if (login.Equals(reader["login"]))
                                return true;
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        public AuthorizationInfo GetAuthorizationInfo(int id)
        {
            try
            {
                using (var command = new NpgsqlCommand($"SELECT password FROM users WHERE Id='{id}'", _connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) return new AuthorizationInfo
                        {
                            UserInfo = { Id = 0 },
                            PasswordHash = 0
                        };

                        return new AuthorizationInfo
                        {
                            UserInfo = { Id = id },
                            PasswordHash = reader.GetInt32(0)
                        };
                    }
                }
            }
            catch
            {

            }

            return new AuthorizationInfo
            {
                UserInfo = { Id = 0 },
                PasswordHash = 0
            };
        }
        public async Task<AuthorizationInfo> GetAuthorizationInfoAsync(int id)
        {
            try
            {
                using (var command = new NpgsqlCommand($"SELECT Password FROM Users WHERE Id='{id}'", _connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync()) return new AuthorizationInfo
                        {
                            UserInfo = { Id = 0 },
                            PasswordHash = 0
                        };

                        return new AuthorizationInfo
                        {
                            UserInfo = { Id = id },
                            PasswordHash = reader.GetInt32(0)
                        };
                    }
                }
            }
            catch
            {

            }

            return new AuthorizationInfo
            {
                UserInfo = { Id = 0 },
                PasswordHash = 0
            };
        }
    }
}
