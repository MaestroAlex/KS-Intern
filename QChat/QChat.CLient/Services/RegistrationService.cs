using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common;

namespace QChat.CLient.Services
{
    class RegistrationService
    {
        public RegistrationService()
        {

        }

        public async Task<RegistrationResult> Register(string login, int passwordHash)
        {
            var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connect();

            if (connection == null) return RegistrationResult.Fail;

            var requestHeaderBytes = new RequestHeader(RequestIntention.Registration).AsBytes();
            var registrationBytes = new RegistrationInfo()
            {
                Login = login,
                PasswordHash = passwordHash
            }.AsBytes();

            Task.WaitAll(
            connection.LockReadAsync(),
            connection.LockWriteAsync()
            );

            try
            {
                await connection.WriteAsync(requestHeaderBytes, 0, RequestHeader.ByteLength);
                await connection.WriteAsync(registrationBytes, 0, registrationBytes.Length);

                var responceInfo = RegistrationResponce.FromConnection(connection);

                return responceInfo.Result;
            }
            finally
            {
                connection.ReleaseRead();
                connection.ReleaseWrite();
            }
        }
    }
}
