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
        private AuthorizationInfo _authorizationInfo;

        private AuthorizationVM _authorizationVM;


        public bool AuthorizationInfoUpdated { get; set; }

        public AuthorizationService(AuthorizationVM authorizationVM)
        {
            _authorizationVM = authorizationVM;
            AuthorizationInfoUpdated = true;
        }


        public bool Authorize(IConnection connection)
        {
            if (connection == null) return false;

            var requestHeader = new RequestHeader(RequestIntention.Authorization);

            try
            {
                connection.LockWrite();
                connection.LockRead();

                if (!connection.Write(requestHeader.AsBytes(), 0, RequestHeader.ByteLength)) return false;
                if (!connection.Write(_authorizationInfo.AsBytes(), 0, AuthorizationInfo.ByteLength)) return false;

                try
                {
                    var authorizationResponceInfo = AuthorizationResponceInfo.FromConnection(connection);
                    return authorizationResponceInfo.Success;
                }
                catch
                {
                    return false;
                }
            }
            finally
            {
                connection.ReleaseRead();
                connection.ReleaseWrite();
            }
        }
        public async Task<bool> AuthorizeAsync(IConnection connection)
        {
            if (connection == null) return false;

            var requestHeader = new RequestHeader(RequestIntention.Authorization);

            try
            {
                var writeLock = connection.LockWriteAsync();
                var readLock = connection.LockReadAsync();

                await writeLock;
                await readLock;

                if (!await connection.WriteAsync(requestHeader.AsBytes(), 0, RequestHeader.ByteLength)) return false;
                if (!await connection.WriteAsync(_authorizationInfo.AsBytes(), 0, AuthorizationInfo.ByteLength)) return false;

                try
                {
                    var authorizationResponceInfo = await AuthorizationResponceInfo.FromConnectionAsync(connection);
                    return authorizationResponceInfo.Success;
                }
                catch
                {
                    return false;
                }
            }
            finally
            {
                connection.ReleaseRead();
                connection.ReleaseWrite();
            }
        }

        public void UpdateAuthorizationInfo(UserInfo userInfo, int passwordHash)
        {
            _authorizationInfo.UserInfo = userInfo;
            _authorizationInfo.PasswordHash = passwordHash;
        }
    }
}
