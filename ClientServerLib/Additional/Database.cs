using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using ClientServerLib.Common;
using ClientServerLib.Additional;
using System.Threading;

namespace ClientServerLib.Additional
{
    class Database
    {
        static NpgsqlConnection connection;
        static Mutex mutex = new Mutex();

        public static async Task<bool> OpenConnection()
        {
            try
            {
                string connstring = "Server=127.0.0.1; Port=5432; User Id=postgres; Password=12345; Database=chatdb;";
                connection = new NpgsqlConnection(connstring);
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<int> RegisterNewUser(string login, string password)
        {
            try
            {
                NpgsqlCommand command = new NpgsqlCommand($"INSERT INTO users (login, password) VALUES ('{login}', '{password}') RETURNING id", connection);
                using (NpgsqlDataReader dataReader = await command.ExecuteReaderAsync())
                {
                    await dataReader.ReadAsync();
                    return (int)dataReader[0];
                }
            }
            catch
            {
                return -1;
            }
        }

        public static async Task<int> LoginUser(string login, string password)
        {
            try
            {
                NpgsqlCommand command = new NpgsqlCommand($"SELECT id FROM users WHERE login LIKE '{login}' AND password LIKE '{password}'", connection);
                using (NpgsqlDataReader dataReader = await command.ExecuteReaderAsync())
                {
                    await dataReader.ReadAsync();
                    return (int)dataReader[0];
                }
            }
            catch
            {
                return -1;
            }
        }

        public static async Task<int> AddRoom(string roomName)
        {
            try
            {
                NpgsqlCommand command = new NpgsqlCommand($"INSERT INTO room (name) VALUES ('{roomName}') RETURNING id", connection);
                using (NpgsqlDataReader dataReader = await command.ExecuteReaderAsync())
                {
                    await dataReader.ReadAsync();
                    return (int)dataReader[0];
                }
            }
            catch
            {
                return -1;
            }
        }

        public static async Task<bool> AddMessage(string message, ClientObject fromClient, string chatRoomName)
        {
            if (fromClient == null || fromClient.ActiveChatRoom == null)
                return false;
            try
            {
                mutex.WaitOne(1500);
                Task.Run(async () =>
                {
                    NpgsqlCommand command = new NpgsqlCommand($"INSERT INTO message (user_id, room_id, message) " +
                            $"VALUES ({fromClient.Id}, (SELECT id FROM room WHERE name = '{chatRoomName}'), '{message}');", connection);
                    using (NpgsqlDataReader dataReader = await command.ExecuteReaderAsync()) { }
                }).Wait();
                mutex.ReleaseMutex();
                return true;
            }
            catch
            {
                mutex.ReleaseMutex();
                return false;
            }
        }

        public static async Task<bool> GiveInvite(string clientName, string roomName)
        {
            try
            {
                NpgsqlCommand command = new NpgsqlCommand($"INSERT INTO user_in_room (user_id, room_id) VALUES " +
                            $"((SELECT id FROM users WHERE login LIKE '{clientName}'), " +
                            $"(SELECT id FROM room WHERE name LIKE '{roomName}'))", connection);
                using (NpgsqlDataReader dataReader = await command.ExecuteReaderAsync()) { }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task CloseConnection()
        {
            await connection.CloseAsync();
        }

        internal static async Task AddClientRoomsToCollection(ClientObject client, Dictionary<int, ChatRoom> rooms)
        {
            try
            {
                NpgsqlCommand command = new NpgsqlCommand($"SELECT r.* FROM room r JOIN user_in_room ur ON ur.room_id=r.id WHERE user_id = {client.Id}", connection);
                using (NpgsqlDataReader dataReader = await command.ExecuteReaderAsync())
                {
                    while (dataReader.Read())
                    {
                        int roomId = (int)dataReader[0];
                        if (!rooms.ContainsKey(roomId))
                        {
                            rooms.Add(roomId, new ChatRoom((string)dataReader[1]));
                        }
                        rooms[roomId].AddClientToRoom(client);
                    }
                }
            }
            catch { }
        }

        internal static async Task<string> GetMessagesFromRoom(string roomName)
        {
            try
            {
                string messagesFromRoom = "";
                NpgsqlCommand command = new NpgsqlCommand($"SELECT u.login, m.message FROM message m JOIN users u ON u.id=m.user_id JOIN room r ON r.id=m.room_id " +
                    $"WHERE r.name='{roomName}' LIMIT 100;", connection);
                using (NpgsqlDataReader dataReader = await command.ExecuteReaderAsync())
                {
                    while (dataReader.Read())
                    {
                        messagesFromRoom += (string)dataReader[0] + ChatSyntax.MessageDiv + (string)dataReader[1] + "\n";
                    }
                }
                return messagesFromRoom;
            }
            catch { return null; }
        }
    }
}
