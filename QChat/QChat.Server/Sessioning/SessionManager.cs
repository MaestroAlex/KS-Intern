using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;
using QChat.Server.Messaging;
using QChat.Common;

namespace QChat.Server.Sessioning
{
    class SessionManager
    {
        private UserManager _userManager;
        private IManagerProvider _managerProvider;

        public SessionManager(IManagerProvider managerProvider)
        {
            _managerProvider = managerProvider;
            _userManager = managerProvider.Get<UserManager>();
        }

        public void StartSession(Connection connection, UserInfo userInfo)
        {
            var session = new Session(connection, _managerProvider, userInfo);

            _userManager.RegisterSession(userInfo, session);

            Console.WriteLine("Starting Session");
            session.SessionClosed += HandleClosedSession;
            session.Start();
        }

        private void HandleClosedSession(Session sender, EventArgs eventArgs)
        {
            _userManager.UnregisterSession(sender);
            Console.WriteLine($"Session closed {sender.Id}");
        }
    }
}
