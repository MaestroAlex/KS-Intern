using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common;


namespace QChat.Server.Sessioning
{
    class UserManager
    {
        public Dictionary<ulong, User> _activeUsers;        

        public void RegisterSession(UserInfo userInfo, Session session)
        {
            if (!_activeUsers.TryGetValue(userInfo.Id, out User user))
            {
                user = new User();
                _activeUsers.Add(userInfo.Id, user);
            }

            user.AddSession(session);
        }

        public User GetUser(ulong id)
        {
            _activeUsers.TryGetValue(id, out User user);
            return user;
        }
    }
}
