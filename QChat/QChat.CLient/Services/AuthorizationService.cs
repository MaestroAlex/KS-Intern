using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using QChat.Common.Net;
using QChat.Common;
using QChat.CLient.ViewModels;

namespace QChat.CLient.Services
{
    class AuthorizationService
    {
        public AuthorizationInfo AuthorizationInfo { get; set; }
        public bool Authorized { get; private set; }

        private AuthorizationVM _authorizationVM;


        public bool AuthorizationInfoUpdated { get; set; }

        public AuthorizationService(AuthorizationVM authorizationVM)
        {
            _authorizationVM = authorizationVM;
            AuthorizationInfoUpdated = true;
        }


        public AuthorizationResult Authorize(IConnection connection)
        {
            if (connection == null) return AuthorizationResult.Fail;

            var requestHeader = new RequestHeader(RequestIntention.Authorization);

            Task.WaitAll(
                connection.LockReadAsync(),
                connection.LockWriteAsync()
                );

            try
            {
                connection.Write(requestHeader.AsBytes(), 0, RequestHeader.ByteLength);
                connection.Write(AuthorizationInfo.AsBytes(), 0, AuthorizationInfo.ByteLength);

                
                var authorizationResponceInfo = AuthorizationResponce.FromConnection(connection);

                Authorized = authorizationResponceInfo.Result == AuthorizationResult.Success;

                return authorizationResponceInfo.Result;
            }
            finally
            {
                connection.ReleaseRead();
                connection.ReleaseWrite();
            }
        }
        public async Task<AuthorizationResult> AuthorizeAsync(IConnection connection)
        {
            if (connection == null) return AuthorizationResult.Fail;

            var requestHeader = new RequestHeader(RequestIntention.Authorization);

            Task.WaitAll(
                connection.LockReadAsync(),
                connection.LockWriteAsync()
                );

            try
            {
                await connection.WriteAsync(requestHeader.AsBytes(), 0, RequestHeader.ByteLength);
                await connection.WriteAsync(AuthorizationInfo.AsBytes(), 0, AuthorizationInfo.ByteLength);               

                var authorizationResponceInfo = await AuthorizationResponce.FromConnectionAsync(connection);

                Authorized = authorizationResponceInfo.Result == AuthorizationResult.Success;

                return authorizationResponceInfo.Result;
            }
            finally
            {
                connection.ReleaseRead();
                connection.ReleaseWrite();
            }
        }

        public void UpdateAuthorizationInfo(UserInfo userInfo, int passwordHash)
        {
            AuthorizationInfo = new AuthorizationInfo
            {
                UserInfo = userInfo,
                PasswordHash = passwordHash,
            };
        }
    }
}
