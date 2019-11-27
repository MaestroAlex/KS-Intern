using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common;
using QChat.Common.Net;


namespace QChat.Server.Sessioning
{
    class UserManager
    {
        public Dictionary<int, User> _activeUsers;      
        

        public UserManager()
        {
            _activeUsers = new Dictionary<int, User>();
        }

        public void RegisterSession(UserInfo userInfo, Session session)
        {
            lock (_activeUsers)
            {
                if (!_activeUsers.TryGetValue(userInfo.Id, out var user))
                {
                    user = new User();
                    _activeUsers.Add(userInfo.Id, user);
                }

                user.AddSession(session);
            }
        }
        public void UnregisterSession(Session session)
        {
            lock (_activeUsers)
            {
                var user = _activeUsers[session.UserId];
                user.RemoveSession(session);
                if (user.Sessions.Count() == 0) _activeUsers.Remove(user.Info.Id);
            }
        }

        public User GetUser(int id)
        {
            lock (_activeUsers)
            {
                _activeUsers.TryGetValue(id, out var user);
                return user;
            }
        }
    }
}
