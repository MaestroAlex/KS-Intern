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
        public string Name { get; private set; }

        private Dictionary<int, Dictionary<int, Session>> _members;

        public Room(int id, string name)
        {
            Id = id;
            IsPublic = true;
            Name = name;
            _members = new Dictionary<int, Dictionary<int, Session>>();
        }
        public Room(int id, string name, bool isPublic)
        {
            Id = id;
            IsPublic = isPublic;
            Name = name;
            _members = new Dictionary<int, Dictionary<int, Session>>();
        }
        public Room(int id, string name, bool isPublic, IEnumerable<UserInfo> members)
        {
            Id = id;
            _members = new Dictionary<int, Dictionary<int, Session>>();
            IsPublic = isPublic;
            Name = name;

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
            var userSessions = _members[sender.UserId];
            lock (userSessions)
            {
                userSessions.Remove(sender.Id);
            }
        }

        public bool MemberConnected(int id, int sessionId)
        {
            if (!_members.TryGetValue(id, out var sessions)) return false;

            return sessions.ContainsKey(sessionId);
        }

        public void DisconnectMember(int id, int sessionId)
        {
            _members[id].Remove(sessionId);
        }

        public IEnumerable<Dictionary<int, Session>> GetMembersSessions()
        {
            var result = from idSessions in _members
                         select idSessions.Value;

            return result;
        }
    }
}
