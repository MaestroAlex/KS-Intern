using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitPackage
{
    public enum ActionEnum
    {
        login, logout,
        get_users,
        send_message, receive_message,
        user_logged_in, user_logged_out,
        connection_check, ok, bad
    }
}
