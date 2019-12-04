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
        private RoomDataManager _roomDataManager;

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

                if (await _userDataManager.UserRegisteredAsync(registrationInfo.Login))
                {
                    await connection.WriteAsync(
                        new RegistrationResponce
                        {
                            Result = RegistrationResult.NicknameAlreadyRegistered,
                            UserInfo = UserInfo.Null
                        }.AsBytes(),
                        0,
                        RegistrationResponce.ByteLength);

                    return;
                }

                if (!await _userDataManager.RegisterUserAsync(registrationInfo.Login, registrationInfo.PasswordHash))
                {
                    await connection.WriteAsync(
                        new RegistrationResponce
                        {
                            Result = RegistrationResult.Fail,
                            UserInfo = UserInfo.Null
                        }.AsBytes(),
                        0,
                        RegistrationResponce.ByteLength);

                    return;
                }

                await connection.WriteAsync(
                        new RegistrationResponce
                        {
                            Result = RegistrationResult.Success,
                            UserInfo = { Id = registrationInfo.Login.GetHashCode() }
                        }.AsBytes(),
                        0,
                        RegistrationResponce.ByteLength);
            }
            catch(Exception e)
            {
                return;
            }
            finally
            { 
                connection.Close();
            }

            await _roomDataManager.AddMemberToRoomAsync(registrationInfo.Login.GetHashCode(), 2);
        }     
    }
}
