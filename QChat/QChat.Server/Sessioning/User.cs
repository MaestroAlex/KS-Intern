using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common;
using QChat.Common.Net;
using QChat.Server.Sessioning;

namespace QChat.Server
{
    class User
    {
        private Dictionary<int, Session> _activeSessions;

        public UserInfo Info { get; protected set; }
        public IEnumerable<Session> Sessions { get => _activeSessions.Values; }

        public User()
        {
            _activeSessions = new Dictionary<int, Session>(2);
        }        

        public void AddSession(Session session)
        {
            lock (_activeSessions)
            {
                _activeSessions.Add(session.Id, session);
                session.SessionClosed += HandleClosedSession;
            }
        }        
        public void RemoveSession(Session session)
        {
            lock (_activeSessions)
            {
                _activeSessions.Remove(session.Id);
            }
        }

        public void HandleClosedSession(Session sender, EventArgs args)
        {
            RemoveSession(sender);
        }
    }
}
