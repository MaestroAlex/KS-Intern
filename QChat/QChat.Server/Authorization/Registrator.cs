using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Server.DataManagment;
using QChat.Common;
using QChat.Common.Net;

namespace QChat.Server.Authorization
{
    class Registrator
    {
        private UserDataManager _userDataManager;

        public Registrator()
        {
            _userDataManager = new UserDataManager();
        }

        public void Register(IConnection connection)
        {
            Task.Run(() => DoRegistration(connection));
        }

        private async void DoRegistration(IConnection connection)
        {
            RegistrationInfo registrationInfo;

            try
            {
                registrationInfo = await RegistrationInfo.FromConnectionAsync(connection);
            }
            catch
            {
                return;
            }

            var headerTask = connection.WriteAsync(new ResponceHeader(ResponceIntention.Registration).AsBytes(), 0, ResponceHeader.ByteLength);

            if (await _userDataManager.UserRegisteredAsync(registrationInfo.Login))
            {
                if (await headerTask)
                {
                    await connection.WriteAsync(
                        new RegistrationResponceInfo
                        {
                            Success = false,
                            UserInfo = UserInfo.Null
                        }.AsBytes(),
                        0,
                        RegistrationResponceInfo.ByteLength);
                }

                return;
            }

            if (!await _userDataManager.RegisterUserAsync(registrationInfo.Login, registrationInfo.PasswordHash))
            {
                if (await headerTask)
                {
                    await connection.WriteAsync(
                        new RegistrationResponceInfo
                        {
                            Success = false,
                            UserInfo = UserInfo.Null
                        }.AsBytes(),
                        0,
                        RegistrationResponceInfo.ByteLength);
                }

                return;
            }
            
            if (await headerTask)
            {
                await connection.WriteAsync(
                        new RegistrationResponceInfo
                        {
                            Success = true,
                            UserInfo = { Id = registrationInfo.Login.GetHashCode() }
                        }.AsBytes(),
                        0,
                        RegistrationResponceInfo.ByteLength);
            }

            connection.Dispose();
        }        
    }
}
