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

                if (info.Equals(verificationInfo))
                {
                    SendAuthorizationResponce(connection, new AuthorizationResponceInfo { Success = true });
                    return AuthorizationResultInfo.GetSuccessfullResult(ref info.UserInfo);
                }
                else
                {
                    SendAuthorizationResponce(connection, new AuthorizationResponceInfo { Success = false });
                    return AuthorizationResultInfo.GetFailedResult(null);
                }
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

                if (info.Equals(verificationInfo))
                {
                    await SendAuthorizationResponceAsync(connection, new AuthorizationResponceInfo { Success = true });
                    return AuthorizationResultInfo.GetSuccessfullResult(ref info.UserInfo);
                }
                else
                {
                    await SendAuthorizationResponceAsync(connection, new AuthorizationResponceInfo { Success = false });
                    return AuthorizationResultInfo.GetFailedResult(null);
                }
            }
            catch
            {
                return AuthorizationResultInfo.GetFailedResult(null);
            }
        }

        private void SendAuthorizationResponce(IConnection connection, AuthorizationResponceInfo responceInfo)
        {
            var responceInfoBytes = responceInfo.AsBytes();
            connection.Write(responceInfoBytes, 0, AuthorizationResponceInfo.ByteLength);
        }
        private async Task SendAuthorizationResponceAsync(IConnection connection, AuthorizationResponceInfo responceInfo)
        {
            var responceInfoBytes = responceInfo.AsBytes();
            await connection.WriteAsync(responceInfoBytes, 0, AuthorizationResponceInfo.ByteLength);
        }
    }
}
