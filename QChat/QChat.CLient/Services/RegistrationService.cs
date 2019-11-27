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

        public async Task<bool> Register(string login, int passwordHash)
        {
            var connection = StaticProvider.GetInstanceOf<NetworkingService>().Connect();

            if (connection == null) return false;

            Task.WaitAll(
            connection.LockReadAsync(),
            connection.LockWriteAsync()
            );

                var requestHeaderBytes = new RequestHeader(RequestIntention.Registration).AsBytes();

                if (!await connection.WriteAsync(requestHeaderBytes, 0, RequestHeader.ByteLength))
                    return false;

                var registrationBytes = new RegistrationInfo()
                {
                    Login = login,
                    PasswordHash = passwordHash
                }.AsBytes();

                if (!await connection.WriteAsync(registrationBytes, 0, registrationBytes.Length)) return false;

                connection.ReleaseWrite();

                RegistrationResponceInfo responceInfo;

                try
                {
                    var responceHeader = ResponceHeader.FromConnection(connection);
                    responceInfo = RegistrationResponceInfo.FromConnection(connection);
                }
                catch
                {
                    return false;
                }


                connection.ReleaseRead();
                return responceInfo.Success;
        }
    }
}
