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
using System.Diagnostics;

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

        public async Task Start()
        {
            if (!_initialized) throw new InvalidOperationException("Server is not initialized");

            _connectionManager.Start(200);

            _continue = true;

            Console.WriteLine("Initialized. Starting Work");

            await Task.Run(Work);
        }

        private async Task Work()
        {
            while (_continue)
            {
                var connection = _connectionManager.GetConnection();

                var authorizationResult = await _authorizationManager.TryAuthorizeAsync(connection);

                if (authorizationResult.Result == Authorization.AuthorizationResult.Registration)
                    continue;
                if (authorizationResult.Result == Authorization.AuthorizationResult.Fail)
                {
                    connection.Close();
                    continue;
                }

                _sessionManager.StartSession(connection, authorizationResult.UserInfo);
            }              
        }

        private void InitializeManagerProvider(IPAddress ipAddress, int port)
        {
            _managerProvider.Register(new UserManager());
            _managerProvider.Register(new GroupManager());
            _managerProvider.Register(new RoomManager());
            _managerProvider.Register(new ConnectionManager(ipAddress, port));
            _managerProvider.Register(new AuthorizationManager(new Authorizator(), new Registrator()));

            //Manager provider needed for initialization, so registered last
            _managerProvider.Register(new MessagingManager(_managerProvider, new ContentRecieverTable(), new ContentSenderTable()));
            _managerProvider.Register(new SessionManager(_managerProvider));
        }
    }
}
