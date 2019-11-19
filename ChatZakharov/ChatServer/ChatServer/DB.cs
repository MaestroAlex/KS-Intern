using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitPackage;

namespace ChatServer
{
    public class DB : IDisposable
    {
        private static NpgsqlConnection conn;
        private DB() { }
        static DB()
        {
            conn = new NpgsqlConnection();
            conn.ConnectionString = "Server=127.0.0.1;Port=5432;Database=Chat;User Id=postgres;Password=jaffa11001;";
            conn.Open();
        }
        public void Dispose() => conn.Close();

        public static async Task<bool> CreateUser(string login, string hashPass, string salt)
        {
            try
            {
                using (var cmd = new NpgsqlCommand("INSERT INTO users (login, hash, salt) VALUES(@login, @pass, @salt)", conn))
                {
                    cmd.Parameters.AddWithValue("login", login);
                    cmd.Parameters.AddWithValue("pass", hashPass);
                    cmd.Parameters.AddWithValue("salt", salt);
                    await cmd.ExecuteNonQueryAsync();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static async Task<ActionEnum> CreateRoom(string owner, string hashPass, string salt, string name)
        {
            try
            {
                int ownerId = -1;
                using (var cmd = new NpgsqlCommand("SELECT id FROM users WHERE login = @owner", conn))
                {
                    cmd.Parameters.AddWithValue("owner", owner);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            ownerId = reader.GetInt32(0);

                        else
                            return ActionEnum.bad_login;
                    }
                }

                bool roomExist = false;
                using (var cmd = new NpgsqlCommand("SELECT id FROM rooms WHERE name = @name", conn))
                {
                    cmd.Parameters.AddWithValue("name", name);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            roomExist = true;
                    }
                }

                if (roomExist)
                    return ActionEnum.room_exist;

                using (var cmd = new NpgsqlCommand("INSERT INTO rooms (owner_user_id, hash, salt, name) VALUES(@ownerId, @pass, @salt, @name)", conn))
                {
                    cmd.Parameters.AddWithValue("ownerId", ownerId);
                    cmd.Parameters.AddWithValue("pass", hashPass ?? "");
                    cmd.Parameters.AddWithValue("salt", salt ?? "");
                    cmd.Parameters.AddWithValue("name", name);
                    await cmd.ExecuteNonQueryAsync();
                }
                return ActionEnum.ok;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return ActionEnum.bad;
            }
        }

        public static async Task<ActionEnum> UserLogin(string login, string hashPass)
        {
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT * FROM users WHERE login = @login", conn))
                {
                    cmd.Parameters.AddWithValue("login", login);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string expectedPass = reader.GetString(2);
                            string salt = reader.GetString(3);
                            hashPass += salt;

                            if (expectedPass.CompareTo(hashPass) == 0)
                                return ActionEnum.ok;
                            else
                                return ActionEnum.wrong_pass;
                        }
                        else
                            return ActionEnum.bad_login;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return ActionEnum.bad;
            }
        }

        public static async Task<List<Channel>> GetChannels(string login)
        {
            List<Channel> res = new List<Channel>();

            try
            {
                using (var cmd = new NpgsqlCommand("SELECT login FROM users WHERE login != @login", conn))
                {
                    cmd.Parameters.AddWithValue("login", login);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            res.Add(new Channel() { Name = reader.GetString(0), Type = ChannelType.user });
                    }
                }

                using (var cmd = new NpgsqlCommand("SELECT name, hash FROM rooms", conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            res.Add(new Channel()
                            {
                                Name = reader.GetString(0),
                                Type = reader.GetString(1) == "" ?
                                    ChannelType.public_open : ChannelType.public_closed
                            });
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return res;
        }

        public static async Task<List<Message>> GetAllHistory(string name)
        {
            {
                List<Message> res = new List<Message>();

                try
                {
                    using (var cmd = new NpgsqlCommand(@"SELECT (SELECT login FROM users WHERE users.id = uch.from_user_id), 
                                                                 u.login,
                                                                 uch.message,
                                                                 uch.datetime
                                                         FROM users u JOIN userchats uch ON u.id = uch.to_user_id
                                                         WHERE u.login = @name OR uch.from_user_id = (SELECT id FROM users WHERE login=@name)", conn))
                    {
                        cmd.Parameters.AddWithValue("name", name);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Message message = new Message()
                                {
                                    From = reader.GetString(0),
                                    To = reader.GetString(1),
                                    Text = reader.GetString(2),
                                    Date = reader.GetDateTime(3)
                                };
                                message.ChatDestination = message.From == name ? message.To : message.From;
                                res.Add(message);
                            }
                        }
                    }

                    using (var cmd = new NpgsqlCommand(@"SELECT (SELECT login FROM users WHERE users.id = rch.from_user_id), 
	                                                            (SELECT name FROM rooms WHERE rooms.id = rch.to_room_id),
	                                                            rch.message,
	                                                            rch.datetime
                                                        FROM users u JOIN roomchats rch ON u.id = rch.from_user_id
                                                        JOIN rooms r ON r.id = rch.to_room_id", conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                                res.Add(new Message()
                                {
                                    From = reader.GetString(0),
                                    To = reader.GetString(1),
                                    ChatDestination = reader.GetString(1),
                                    Text = reader.GetString(2),
                                    Date = reader.GetDateTime(3)
                                });
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                return res;
            }
        }

        public static async Task<bool> IsRoom(string name)
        {
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT id FROM rooms WHERE name = @name", conn))
                {
                    cmd.Parameters.AddWithValue("name", name);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return true;
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        public static async Task<bool> SendToRoom(NetworkMessage message)
        {
            try
            {
                int fromUserId = -1;
                int toRoomId = -1;

                using (var cmd = new NpgsqlCommand("SELECT id FROM users WHERE login = @login", conn))
                {
                    cmd.Parameters.AddWithValue("login", ((Message)message.Obj).From);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            fromUserId = reader.GetInt32(0);
                    }
                }

                using (var cmd = new NpgsqlCommand("SELECT id FROM rooms WHERE name = @name", conn))
                {
                    cmd.Parameters.AddWithValue("name", ((Message)message.Obj).To);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            toRoomId = reader.GetInt32(0);
                    }
                }

                using (var cmd = new NpgsqlCommand("INSERT INTO roomchats (from_user_id, to_room_id, message, datetime) VALUES(@fromUserId, @toRoomId, @message, @datetime)", conn))
                {
                    cmd.Parameters.AddWithValue("fromUserId", fromUserId);
                    cmd.Parameters.AddWithValue("toRoomId", toRoomId);
                    cmd.Parameters.AddWithValue("message", ((Message)message.Obj).Text);
                    cmd.Parameters.AddWithValue("datetime", DateTime.Now.ToUniversalTime());
                    await cmd.ExecuteNonQueryAsync();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        public static async Task<bool> SendToUser(NetworkMessage message)
        {
            try
            {
                int fromUserId = -1;
                int toUserId = -1;

                using (var cmd = new NpgsqlCommand("SELECT id FROM users WHERE login = @login", conn))
                {
                    cmd.Parameters.AddWithValue("login", ((Message)message.Obj).From);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            fromUserId = reader.GetInt32(0);
                    }
                }

                using (var cmd = new NpgsqlCommand("SELECT id FROM users WHERE login = @login", conn))
                {
                    cmd.Parameters.AddWithValue("login", ((Message)message.Obj).To);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            toUserId = reader.GetInt32(0);
                    }
                }

                using (var cmd = new NpgsqlCommand("INSERT INTO userchats (from_user_id, to_user_id, message, datetime) VALUES(@fromUserId, @toUserId, @message, @datetime)", conn))
                {
                    cmd.Parameters.AddWithValue("fromUserId", fromUserId);
                    cmd.Parameters.AddWithValue("toUserId", toUserId);
                    cmd.Parameters.AddWithValue("message", ((Message)message.Obj).Text);
                    cmd.Parameters.AddWithValue("datetime", DateTime.Now.ToUniversalTime());
                    await cmd.ExecuteNonQueryAsync();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }
    }
}

