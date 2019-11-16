using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.Server.Authorization
{
    interface IAuthorizator
    {
        AuthorizationResult Authorize(Connection connection);
        Task<AuthorizationResult> AuthorizeAsync(Connection connection);
    }
}