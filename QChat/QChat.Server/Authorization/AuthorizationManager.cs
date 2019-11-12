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
        private Authorizator _authorizator;


        public AuthorizationManager(Authorizator authorizator)
        {
            _authorizator = authorizator;
        }

        public AuthorizationResult TryAuthorize(Connection connection)
        {
            RequestHeader header;

            try
            {
                header = RequestHeader.FromConnection(connection);
            }
            catch
            {
                return AuthorizationResult.GetFailedAuthorization(null);
            }

            switch (header.Intention)
            {
                case RequestIntention.Authorization:
                    return _authorizator.Authorize(connection);
                default:
                    return AuthorizationResult.GetFailedAuthorization(null);
            }
        }
        public async Task<AuthorizationResult> TryAuthorizeAsync(Connection connection)
        {
            RequestHeader header;

            try
            {
                header = await RequestHeader.FromConnectionAsync(connection);          
            }
            catch
            {
                return AuthorizationResult.GetFailedAuthorization(null);
            }

            switch (header.Intention)
            {
                case RequestIntention.Authorization:
                    return _authorizator.Authorize(connection);
                default:
                    return AuthorizationResult.GetFailedAuthorization(null);
            }
        }        
    }
}
