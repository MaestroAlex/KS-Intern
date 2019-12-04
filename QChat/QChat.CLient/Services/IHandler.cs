using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common.Net;

namespace QChat.CLient.Services
{
    interface IHandler
    {
        void Handle(IConnection connection);
        Task HandleAsync(IConnection connection);
    }
}
