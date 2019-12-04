using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using QChat.Common;
using QChat.Server.Messaging;

namespace QChat.Server.DataManagment
{
    class RoomDataManager
    {
        private NpgsqlConnection _connection;

        public RoomDataManager()
        {
            _connection = DBConnectionProvider.Instance.Connection;
        }


        public int RegisterRoom(string name, bool isPublic)
        {
            try
            {
                using (var command = new NpgsqlCommand($"INSERT INTO rooms(name, is_public) VALUES ('{name}',{isPublic}) RETURNING id", _connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        return reader.GetInt32(0);
                    }                    
                }                      
            }
            catch(Exception e)
            {

            }
            return -1;
        }
        public async Task<int> RegisterRoomAsync(string name)
        {
            try
            {
                using (var command = new NpgsqlCommand($"INSERT INTO Rooms(Id, Name) VALUES ({name.GetHashCode()},'{name}') RETURNING id", _connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        return reader.GetInt32(0);
                    }
                }
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
                using (var command = new NpgsqlCommand($"SELECT name FROM rooms WHERE Id={id}", _connection))
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
        public async Task<string> GetRoomNameAsync(int id)
        {
            try
            {
                using (var command = new NpgsqlCommand($"SELECT name FROM Rooms WHERE Id={id}", _connection))
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

        public bool AddMemberToRoom(int userId, int roomId)
        {
            try
            {
                using (var command = new NpgsqlCommand($"INSERT INTO room_members(room_id, user_id) VALUES ({roomId},{userId})", _connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }
        public async Task<bool> AddMemberToRoomAsync(int userId, int roomId)
        {
            try
            {
                using (var command = new NpgsqlCommand($"INSERT INTO room_members(room_id, user_id) VALUES ({roomId},{userId})", _connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }

        public async Task<IEnumerable<RoomSynchronizationInfo>> GetUserRoomsAsync(int userId)
        {
            var result = new List<RoomSynchronizationInfo>();

            try
            {
                using (var command = new NpgsqlCommand($"SELECT id, name, is_public FROM rooms WHERE id IN (SELECT room_id FROM room_members WHERE user_id={userId})", _connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(
                            new RoomSynchronizationInfo(
                                reader.GetBoolean(2),
                                reader.GetInt32(0),
                                reader.GetString(1)
                                ));
                        }
                    }
                }
            }
            catch(Exception e)
            {

            }

            return null;
        }
        public IEnumerable<RoomSynchronizationInfo> GetUserRooms(int userId)
        {
            var result = new List<RoomSynchronizationInfo>();

            try
            {
                using (var command = new NpgsqlCommand($"SELECT id, name, is_public FROM rooms WHERE id IN (SELECT room_id FROM room_members WHERE user_id={userId})", _connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(
                            new RoomSynchronizationInfo(
                                reader.GetBoolean(2),
                                reader.GetInt32(0),
                                reader.GetString(1)
                                ));
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return result;
        }

        public Dictionary<int, Room> GetRooms()
        {
            var result = new Dictionary<int, Room>();

            try
            {
                using (var command = new NpgsqlCommand("SELECT id, name, is_public FROM rooms", _connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(reader.GetInt32(0), new Room(reader.GetInt32(0), reader.GetString(1), reader.GetBoolean(2)));
                        }
                    }
                }

                using (var command = new NpgsqlCommand("SELECT room_id, user_id FROM room_members", _connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result[reader.GetInt32(0)].AddMember(new UserInfo { Id = reader.GetInt32(1) });
                        }
                    }
                }

                return result;
            }
            catch(Exception e)
            {

            }

            return null;
        }
    }
}
