using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;
using QChat.Common;

namespace QChat.Server.Authorization
{
    class Authorizator : IAuthorizator
    {
        public AuthorizationResult Authorize(Connection connection)
        {
            try
            {
                var info = AuthorizationInfo.FromConnection(connection);
                return AuthorizationResult.GetSuccessfullAuthorization(ref info.UserInfo);
            }
            catch
            {
                return AuthorizationResult.GetFailedAuthorization(null);
            }
        }
        public async Task<AuthorizationResult> AuthorizeAsync(Connection connection)
        {
            try
            {
                var info = await AuthorizationInfo.FromConnectionAsync(connection);
                return AuthorizationResult.GetSuccessfullAuthorization(ref info.UserInfo);
            }
            catch
            {
                return AuthorizationResult.GetFailedAuthorization(null);
            }
        }
    }
}
