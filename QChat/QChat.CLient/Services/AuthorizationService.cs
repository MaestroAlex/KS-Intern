using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            UpdateAuthorizationInfo();

            var requestHeader = new RequestHeader(RequestIntention.Authorization);

            connection.Write(requestHeader.AsBytes(), 0, RequestHeader.ByteLength);
            connection.Write(_authorizationInfo.AsBytes(), 0, AuthorizationInfo.ByteLength);

            return true;
        }
        public async Task<bool> AuthorizeAsync(IConnection connection)
        {
            var requestHeader = new RequestHeader(RequestIntention.Authorization);

            await connection.WriteAsync(requestHeader.AsBytes(), 0, RequestHeader.ByteLength);
            await connection.WriteAsync(_authorizationInfo.AsBytes(), 0, AuthorizationInfo.ByteLength);

            return true;
        }

        private void UpdateAuthorizationInfo()
        {
            if (AuthorizationInfoUpdated)
                return;


            _authorizationInfo.UserInfo.Id = (ulong)_authorizationVM.Login.GetHashCode();
        }
    }
}
