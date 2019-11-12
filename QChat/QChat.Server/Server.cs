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

namespace QChat.Server
{
    class Server
    {
        private ConnectionManager _connectionManager;
        private AuthorizationManager _authorizationManager;
        private SessionManager _sessionManager;

        private bool _continue;

        public void Initialize(IPAddress ipAdress, int port)
        {
            _connectionManager = new ConnectionManager(ipAdress, port);
            _authorizationManager = new AuthorizationManager(new Authorizator());

            var userManager = new UserManager();

            _sessionManager = new SessionManager(userManager);
        }

        public void Start()
        {
            _connectionManager.Start(200);


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
    }
}
