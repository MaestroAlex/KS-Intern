using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using QChat.Common.Net;
using QChat.Common;
using QChat.Server.Authorization;
using QChat.Server.Sessioning;
using QChat.Server.Messaging;

namespace QChat.Server
{
    public class Server
    {
        private IManagerProvider _managerProvider;

        private ConnectionManager _connectionManager;
        private AuthorizationManager _authorizationManager;
        private SessionManager _sessionManager;

        private bool _initialized = false;
        private bool _continue = false;


        public Server()
        {
            _managerProvider = new ManagerProvider();
        }

        public void Initialize(IPAddress ipAdress, int port)
        {
            InitializeManagerProvider(ipAdress, port);

            _connectionManager = _managerProvider.Get<ConnectionManager>();
            _authorizationManager = _managerProvider.Get<AuthorizationManager>();
            _sessionManager = _managerProvider.Get<SessionManager>();

            _initialized = true;
        }

        public void Start()
        {
            if (!_initialized) throw new InvalidOperationException("Server is not initialized");

            _connectionManager.Start(200);

            _continue = true;
            var workThread = new Thread(new ThreadStart(Work));
            workThread.Start();
        }

        private void Work()
        {
            while (_continue)
            {
               var connection = _connectionManager.GetConnection();

               var authorizationResult = _authorizationManager.TryAuthorize(connection);

                _sessionManager.StartSession(connection, authorizationResult.UserInfo);
            }              
        }

        private void InitializeManagerProvider(IPAddress ipAddress, int port)
        {
            _managerProvider.Register(new ConnectionManager(ipAddress, port));
            _managerProvider.Register(new AuthorizationManager(new Authorizator()));
            _managerProvider.Register(new UserManager());
            _managerProvider.Register(new GroupManager());
            _managerProvider.Register(new RoomManager());
            _managerProvider.Register(new MessagingManager(_managerProvider, new ContentRecieverTable(), new ContentSenderTable()));
            _managerProvider.Register(new SessionManager(_managerProvider));
        }
    }
}
