using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;
using QChat.Common;
using QChat.Server.DataManagment;

namespace QChat.Server.Authorization
{
    class Authorizator : IAuthorizator
    {
        private UserDataManager _userDataManager;

        public Authorizator()
        {
            _userDataManager = new UserDataManager();
        }

        public AuthorizationResultInfo Authorize(IConnection connection)
        {
            try
            {
                var info = AuthorizationInfo.FromConnection(connection);

                var verificationInfo = _userDataManager.GetAuthorizationInfo(info.UserInfo.Id);

                if (info.UserInfo.Id != verificationInfo.UserInfo.Id)
                {
                    SendAuthorizationResponce(connection, new AuthorizationResponce { Result = Common.AuthorizationResult.UserNotFound });
                    return AuthorizationResultInfo.GetFailedResult(null);
                }
                
                if(info.PasswordHash != verificationInfo.PasswordHash)
                {
                    SendAuthorizationResponce(connection, new AuthorizationResponce { Result = Common.AuthorizationResult.IncorrectPassword });
                    return AuthorizationResultInfo.GetFailedResult(null);
                }

                SendAuthorizationResponce(connection, new AuthorizationResponce { Result = Common.AuthorizationResult.Success });
                return AuthorizationResultInfo.GetSuccessfullResult(ref info.UserInfo);
            }
            catch
            {
                return AuthorizationResultInfo.GetFailedResult(null);
            }
        }
        public async Task<AuthorizationResultInfo> AuthorizeAsync(IConnection connection)
        {
            try
            {
                var info = await AuthorizationInfo.FromConnectionAsync(connection);

                var verificationInfo = await _userDataManager.GetAuthorizationInfoAsync(info.UserInfo.Id);

                if (info.UserInfo.Id != verificationInfo.UserInfo.Id)
                {
                    await SendAuthorizationResponceAsync(connection, new AuthorizationResponce { Result = Common.AuthorizationResult.UserNotFound });
                    return AuthorizationResultInfo.GetFailedResult(null);
                }

                if (info.PasswordHash != verificationInfo.PasswordHash)
                {
                    await SendAuthorizationResponceAsync(connection, new AuthorizationResponce { Result = Common.AuthorizationResult.IncorrectPassword });
                    return AuthorizationResultInfo.GetFailedResult(null);
                }

                await SendAuthorizationResponceAsync(connection, new AuthorizationResponce { Result = Common.AuthorizationResult.Success });

                return AuthorizationResultInfo.GetSuccessfullResult(ref info.UserInfo);
            }
            catch
            {
                return AuthorizationResultInfo.GetFailedResult(null);
            }
        }

        private void SendAuthorizationResponce(IConnection connection, AuthorizationResponce responceInfo)
        {
            var responceInfoBytes = responceInfo.AsBytes();
            connection.Write(responceInfoBytes, 0, AuthorizationResponce.ByteLength);
        }
        private async Task SendAuthorizationResponceAsync(IConnection connection, AuthorizationResponce responceInfo)
        {
            var responceInfoBytes = responceInfo.AsBytes();
            await connection.WriteAsync(responceInfoBytes, 0, AuthorizationResponce.ByteLength);
        }
    }
}
