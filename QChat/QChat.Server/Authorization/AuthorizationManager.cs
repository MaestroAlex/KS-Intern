using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using QChat.Common.Net;
using QChat.Server.Sessioning;
using QChat.Common;

namespace QChat.Server.Authorization
{
    class AuthorizationManager
    {
        private IAuthorizator _authorizator;
        private Registrator _registrator;


        public AuthorizationManager(IAuthorizator authorizator, Registrator registrator)
        {
            _authorizator = authorizator;
            _registrator = registrator;
        }

        public AuthorizationResultInfo TryAuthorize(Connection connection)
        {
            RequestHeader header;

            try
            {
                header = RequestHeader.FromConnection(connection);
            }
            catch
            {
                return AuthorizationResultInfo.GetFailedResult(null);
            }

            switch (header.Intention)
            {
                case RequestIntention.Authorization:
                    Console.WriteLine("User authorizing");
                    return _authorizator.Authorize(connection);
                case RequestIntention.Registration:
                    Console.WriteLine("User Registrating");
                    _registrator.Register(connection);
                    return AuthorizationResultInfo.GetRegistrationResult();
                default:
                    return AuthorizationResultInfo.GetFailedResult(null);
            }
        }
        public async Task<AuthorizationResultInfo> TryAuthorizeAsync(Connection connection)
        {
            RequestHeader header;

            try
            {
                header = await RequestHeader.FromConnectionAsync(connection);          
            }
            catch
            {
                return AuthorizationResultInfo.GetFailedResult(null);
            }

            switch (header.Intention)
            {
                case RequestIntention.Authorization:
                    return _authorizator.Authorize(connection);
                default:
                    return AuthorizationResultInfo.GetFailedResult(null);
            }
        }        
    }
}
