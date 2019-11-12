using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Server.Sessioning;
using QChat.Common;

namespace QChat.Server.Authorization
{
    struct AuthorizationResult
    {
        public bool Successful;
        public UserInfo UserInfo;
        public AuthorizationErrorInfo ErrorInfo;

        public static AuthorizationResult GetFailedAuthorization(AuthorizationErrorInfo errorInfo) => new AuthorizationResult
        {
            Successful = false,
            UserInfo = UserInfo.Null,
            ErrorInfo = errorInfo
        };

        public static AuthorizationResult GetSuccessfullAuthorization(ref UserInfo userInfo) => new AuthorizationResult
        {
            Successful = true,
            UserInfo = userInfo,
            ErrorInfo = null
        };
    }
}
