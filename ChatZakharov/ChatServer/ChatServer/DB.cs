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

        public static async Task<bool> CreateDB()
        {
            try
            {
                using (var cmd = new NpgsqlCommand("UPDATE pg_database SET datallowconn = 'false' WHERE datname = 'SimpleChat';", conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var cmd = new NpgsqlCommand(@"SELECT pg_terminate_backend(pg_stat_activity.pid)
                                                     FROM pg_stat_activity
                                                     WHERE pg_stat_activity.datname = 'SimpleChat' AND pid<> pg_backend_pid();", conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var cmd = new NpgsqlCommand("DROP DATABASE IF EXISTS \"SimpleChat\"", conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var cmd = new NpgsqlCommand("CREATE DATABASE \"SimpleChat\"", conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var cmd = new NpgsqlCommand("SELECT datname FROM pg_catalog.pg_database WHERE lower(datname) = lower('SimpleChat');", conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                            throw new Exception("Can't create database");
                    }
                }

                string newConnectionString = Config.GetConfig().ConnectionString + "Database=SimpleChat;";
                conn.Close();
                conn.ConnectionString = newConnectionString;
                conn.Open();

                using (var cmd = new NpgsqlCommand(@"CREATE TABLE public.rooms(
                                                            id SERIAL PRIMARY KEY,
                                                            name character varying(100),
                                                            hash character varying(100),
                                                            salt character varying(100)
                                                     );
                                                      
                                                     CREATE TABLE public.users(
                                                            id SERIAL PRIMARY KEY,
                                                            login character varying(100),
                                                      	    hash character varying(100),
                                                      	    salt character varying(100)
                                                     );
                                                      
                                                    CREATE TABLE public.roomuser(
                                                            id SERIAL PRIMARY KEY,
                                                            user_id integer REFERENCES users(id),
                                                            room_id integer REFERENCES rooms(id)
                                                    );
                                                    
                                                    CREATE TABLE public.message_type(
                                                            id SERIAL PRIMARY KEY,
                                                            name character varying(100) NOT NULL
                                                    );
                                                    CREATE TABLE public.messages(
                                                            id SERIAL PRIMARY KEY,
                                                            message_type_id integer REFERENCES message_type(id),
                                                            content text,
                                                            datetime timestamp without time zone
                                                    );
                                                    
                                                    CREATE TABLE public.userchats(
                                                            id SERIAL PRIMARY KEY,
                                                            from_user_id integer REFERENCES users(id),
                                                            to_user_id integer REFERENCES users(id),
                                                            message_id integer REFERENCES messages(id)
                                                    );
                                                    
                                                    CREATE TABLE public.roomchats(
                                                            id SERIAL PRIMARY KEY,
                                                            from_user_id integer REFERENCES users(id),
                                                            to_room_id integer REFERENCES rooms(id),
                                                            message_id integer REFERENCES messages(id)
                                                    );
                                                    
                                                    INSERT INTO message_type(name)
                                                    VALUES('text'),('image'),('document');", conn))
                {
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

        public static void Initialize(string connectionString)
        {
            conn = new NpgsqlConnection();
            conn.ConnectionString = connectionString;
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

        public static async Task<ActionEnum> CreateRoom(string name, string creator, string hashPass, string salt)
        {
            try
            {
                using (var cmd = new NpgsqlCommand(@"SELECT id FROM rooms WHERE name = @name
                                                     UNION
                                                     SELECT id FROM users WHERE login = @name", conn))
                {
                    cmd.Parameters.AddWithValue("name", name);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return ActionEnum.room_exist;
                    }
                }

                using (var cmd = new NpgsqlCommand(@"INSERT INTO rooms (name, hash, salt)
                                                    VALUES (@name, @pass, @salt)", conn))
                {
                    cmd.Parameters.AddWithValue("pass", hashPass ?? "");
                    cmd.Parameters.AddWithValue("salt", salt ?? "");
                    cmd.Parameters.AddWithValue("name", name);
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var cmd = new NpgsqlCommand(@"INSERT INTO roomuser (user_id, room_id)
                                                    VALUES(
                                                        (SELECT id FROM users WHERE login = @creator),
                                                        (SELECT id FROM rooms WHERE name = @name))", conn))
                {
                    cmd.Parameters.AddWithValue("creator", creator);
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
                using (var cmd = new NpgsqlCommand(@"SELECT id FROM rooms WHERE name = @login", conn))
                {
                    cmd.Parameters.AddWithValue("login", login);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return ActionEnum.room_exist;
                    }
                }


                using (var cmd = new NpgsqlCommand(@"SELECT * FROM users WHERE login = @login", conn))
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
                            res.Add(new Channel()
                            {
                                Name = reader.GetString(0),
                                IsEntered = true,
                                Type = ChannelType.user
                            });
                    }
                }

                using (var cmd = new NpgsqlCommand(@"SELECT name, hash FROM rooms", conn))
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

                using (var cmd = new NpgsqlCommand(@"SELECT name
                                                     FROM rooms r JOIN roomuser ru ON r.id = ru.room_id
                                                     WHERE (SELECT id FROM users WHERE users.login = @login) = ru.user_id", conn))
                {
                    cmd.Parameters.AddWithValue("login", login);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            res.Where((item) => item.Name == reader.GetString(0)).First().IsEntered = true;
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
            List<Message> res = new List<Message>();

            try
            {
                using (var cmd = new NpgsqlCommand(@"SELECT (SELECT login FROM users WHERE users.id = uch.from_user_id),
                                                     	    (SELECT login FROM users WHERE users.id = uch.to_user_id),
                                                            mt.name, m.content, m.datetime
                                                     FROM  userchats uch JOIN messages m ON m.id = uch.message_id
                                                     	                 JOIN message_type mt ON mt.id = m.message_type_id
                                                     WHERE uch.to_user_id = (SELECT id FROM users WHERE login = @name) OR 
                                                     	   uch.from_user_id = (SELECT id FROM users WHERE login = @name)", conn))
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
                                MessageType = DefineMessageType(reader.GetString(2)),
                                Content = reader.GetString(3),
                                Date = reader.GetDateTime(4)
                            };
                            message.ChatDestination = message.From == name ? message.To : message.From;
                            res.Add(message);
                        }
                    }
                }

                using (var cmd = new NpgsqlCommand(@"SELECT (SELECT login FROM users WHERE users.id = rch.from_user_id), 
                                                            (SELECT name FROM rooms WHERE rooms.id = rch.to_room_id),
                                                     		mt.name, m.content, m.datetime
                                                     FROM roomchats rch JOIN messages m ON m.id = rch.message_id 
                                                     				    JOIN message_type mt ON mt.id = m.message_type_id
                                                     WHERE EXISTS (SELECT id FROM roomuser WHERE room_id = rch.to_room_id 					  						   
                                                     			  AND user_id = (SELECT id FROM users WHERE login = @name))", conn))
                {
                    cmd.Parameters.AddWithValue("name", name);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            res.Add(new Message()
                            {
                                From = reader.GetString(0),
                                To = reader.GetString(1),
                                ChatDestination = reader.GetString(1),
                                MessageType = DefineMessageType(reader.GetString(2)),
                                Content = reader.GetString(3),
                                Date = reader.GetDateTime(4)
                            });
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }

            return res;
        }

        public static async Task<List<Message>> GetChannelHistory(string roomName)
        {
            List<Message> res = new List<Message>();

            try
            {
                using (var cmd = new NpgsqlCommand(@"SELECT (SELECT login FROM users WHERE users.id = rch.from_user_id), 
                                                            (SELECT name FROM rooms WHERE rooms.id = rch.to_room_id),
                                                            mt.name, m.content, m.datetime
                                                     FROM roomchats rch JOIN messages m ON m.id = rch.message_id 
                                                                    	   JOIN message_type mt ON mt.id = m.message_type_id
                                                     WHERE rch.to_room_id = (SELECT id FROM rooms WHERE rooms.name = @roomName)", conn))
                {
                    cmd.Parameters.AddWithValue("roomName", roomName);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            res.Add(new Message()
                            {
                                From = reader.GetString(0),
                                To = reader.GetString(1),
                                ChatDestination = reader.GetString(1),
                                MessageType = DefineMessageType(reader.GetString(2)),
                                Content = reader.GetString(3),
                                Date = reader.GetDateTime(4)
                            });
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }

            return res;
        }

        public static async Task<ActionEnum> EnterRoom(string user, string roomName, string roomPass)
        {
            try
            {
                bool AuthorizationResult = false;
                using (var cmd = new NpgsqlCommand(@"SELECT hash, salt 
                                                     FROM rooms 
                                                     WHERE rooms.name = @roomName", conn))
                {
                    cmd.Parameters.AddWithValue("roomName", roomName);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string expectedPass = reader.GetString(0);
                            string salt = reader.GetString(1);

                            roomPass += salt;

                            if (expectedPass.CompareTo(roomPass) == 0)
                                AuthorizationResult = true;
                        }
                        else
                            return ActionEnum.bad;
                    }
                }

                if (AuthorizationResult)
                {
                    using (var cmd = new NpgsqlCommand(@"INSERT INTO roomuser (user_id, room_id)
                                                        VALUES(
                                                            (SELECT id FROM users WHERE login = @user),
                                                            (SELECT id FROM rooms WHERE name = @roomName))", conn))
                    {
                        cmd.Parameters.AddWithValue("user", user);
                        cmd.Parameters.AddWithValue("roomName", roomName);
                        await cmd.ExecuteNonQueryAsync();
                        return ActionEnum.ok;
                    }
                }
                else
                    return ActionEnum.wrong_pass;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return ActionEnum.bad;
            }
        }

        public static async Task<ActionEnum> ExitRoom(string user, string roomName)
        {
            try
            {
                using (var cmd = new NpgsqlCommand(@"DELETE FROM roomuser
                                                    WHERE roomuser.user_id = (SELECT id FROM users WHERE users.login = @user)
                                                    	AND roomuser.room_id = (SELECT id FROM rooms WHERE rooms.name = @roomName)", conn))
                {
                    cmd.Parameters.AddWithValue("user", user);
                    cmd.Parameters.AddWithValue("roomName", roomName);
                    await cmd.ExecuteNonQueryAsync();
                    return ActionEnum.ok;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return ActionEnum.bad;
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

        public static async Task<bool> SendToRoom(Message message)
        {
            try
            {
                int messageId = await WriteInternalMessage(message);

                using (var cmd = new NpgsqlCommand(@"INSERT INTO roomchats (from_user_id, to_room_id, message_id)
                                                    VALUES(
                                                        (SELECT id FROM users WHERE login = @fromUser), 
                                                        (SELECT id FROM rooms WHERE name = @toRoom), @messageId)", conn))
                {
                    cmd.Parameters.AddWithValue("fromUser", message.From);
                    cmd.Parameters.AddWithValue("toRoom", message.To);
                    cmd.Parameters.AddWithValue("messageId", messageId);
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

        public static async Task<bool> SendToUser(Message message)
        {
            try
            {
                int messageId = await WriteInternalMessage(message);

                using (var cmd = new NpgsqlCommand(@"INSERT INTO userchats (from_user_id, to_user_id, message_id)
                                                    VALUES(
                                                        (SELECT id FROM users WHERE login = @fromUser), 
                                                        (SELECT id FROM users WHERE login = @toUser), @messageId)", conn))
                {
                    cmd.Parameters.AddWithValue("fromUser", message.From);
                    cmd.Parameters.AddWithValue("toUser", message.To);
                    cmd.Parameters.AddWithValue("messageId", messageId);
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

        private static async Task<int> WriteInternalMessage(Message message)
        {
            using (var cmd = new NpgsqlCommand(@"INSERT INTO messages (message_type_id, content, datetime)
                                                    VALUES (@message_type, @message, @datetime)
                                                    RETURNING id", conn))
            {
                cmd.Parameters.AddWithValue("message_type", (int)message.MessageType);
                cmd.Parameters.AddWithValue("message", message.Content);
                cmd.Parameters.AddWithValue("datetime", message.Date.ToUniversalTime());
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    return reader.GetInt32(0);
                }

            }
        }

        private static MessageType DefineMessageType(string rawType)
        {
            switch (rawType)
            {
                case "text":
                    return MessageType.text;
                case "image":
                    return MessageType.image;
                case "document":
                    return MessageType.document;
                default:
                    return MessageType.text;
            }
        }
    }
}

