using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;
using QChat.Common;

namespace QChat.Server.Sessioning
{
    class SessionManager
    {
        private UserManager _userManager;

        public SessionManager(UserManager userManager)
        {
            _userManager = userManager;
        }

        public void StartSession(Connection connection, UserInfo userInfo)
        {
            var session = new Session(connection);

            _userManager.RegisterSession(userInfo, session);

            session.Start();
        }
    }
}
