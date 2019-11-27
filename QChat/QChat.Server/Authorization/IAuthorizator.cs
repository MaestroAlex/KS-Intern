using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Server.Authorization
{
    interface IAuthorizator
    {
        AuthorizationResultInfo Authorize(IConnection connection);
        Task<AuthorizationResultInfo> AuthorizeAsync(IConnection connection);
    }
}