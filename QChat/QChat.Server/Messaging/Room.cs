using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Server.Sessioning;
using QChat.Common;

namespace QChat.Server.Messaging
{
    class Room
    {
        public int Id { get; private set; }
        public bool IsPublic { get; private set; }

        private Dictionary<int, Dictionary<int, Session>> _members;

        public Room(int id)
        {
            Id = id;
            IsPublic = true;
            _members = new Dictionary<int, Dictionary<int, Session>>();
        }
        public Room(int id, bool isPublic)
        {
            Id = id;
            IsPublic = isPublic;
            _members = new Dictionary<int, Dictionary<int, Session>>();
        }
        public Room(int id, bool isPublic, IEnumerable<UserInfo> members)
        {
            Id = id;
            _members = new Dictionary<int, Dictionary<int, Session>>();
            IsPublic = isPublic;

            foreach (var info in members)
                _members.Add(info.Id, new Dictionary<int, Session>());
        }

        public void AddMember(UserInfo userInfo)
        {
            _members.Add(userInfo.Id, new Dictionary<int, Session>());
        }
        public bool AddActiveMember(Session session)
        {
            if (!_members.ContainsKey(session.UserId)) return false;

            lock (_members)
            {
                _members[session.UserId].Add(session.Id, session);
                session.SessionClosed += HandleClosedSession;
            }
            return true;
        }

        public bool HasMember(int id)
        {
            return _members.ContainsKey(id);
        }

        private void HandleClosedSession(Session sender, EventArgs eventArgs)
        {
            var userSessions = _members[sender.Id];
            lock (userSessions)
            {
                userSessions.Remove(sender.Id);
            }
        }

        public IEnumerable<Dictionary<int, Session>> GetMembersSessions()
        {
            var result = from idSessions in _members
                         select idSessions.Value;

            return result;
        }
    }
}
