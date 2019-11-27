using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;
using QChat.Common;

namespace QChat.CLient.Services
{
    class SessioningService
    {
        private IConnection _connection;


        public void StartSession(IConnection connection)
        {
            _connection = connection;
            _connection.ConnectionClosed += this.HandleClosedConnection;
            Task.Run(ServeSession);
        }

        private void HandleClosedConnection(IConnection sender, EventArgs args)
        {

        }

        private async void ServeSession()
        {
            while (_connection != null)
            {
                var responceHeader = ResponceHeader.FromConnection(_connection);

                switch (responceHeader.Intention)
                {
                    case ResponceIntention.Messaging:
                        await StaticProvider.GetInstanceOf<MessagingService>().RecieveMessageAsync(_connection);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }       
    }
}
