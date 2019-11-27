using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Server.Sessioning;
using QChat.Common;

namespace QChat.Server.Authorization
{
    struct AuthorizationResultInfo
    {
        public AuthorizationResult Result;
        public UserInfo UserInfo;
        public AuthorizationErrorInfo ErrorInfo;

        public static AuthorizationResultInfo GetFailedResult(AuthorizationErrorInfo errorInfo) => new AuthorizationResultInfo
        {
            Result = AuthorizationResult.Fail,
            UserInfo = UserInfo.Null,
            ErrorInfo = errorInfo
        };

        public static AuthorizationResultInfo GetSuccessfullResult(ref UserInfo userInfo) => new AuthorizationResultInfo
        {
            Result = AuthorizationResult.Authorized,
            UserInfo = userInfo,
            ErrorInfo = null
        };

        public static AuthorizationResultInfo GetRegistrationResult() => new AuthorizationResultInfo
        {
            Result = AuthorizationResult.Registration,
            UserInfo = UserInfo.Null,
            ErrorInfo = null
        };
    }    

    public enum AuthorizationResult
    {
        Fail = 0,
        Authorized,
        Registration
    }
}
