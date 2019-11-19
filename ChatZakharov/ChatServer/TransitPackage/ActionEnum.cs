using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitPackage
{
    public enum ActionEnum
    {
        login, logout, get_connection_check_interval,
        get_channels, get_history, room_exist,
        send_message, receive_message,
        channel_created, channel_deleted,
        create_room, create_user, 
        connection_check, ok, bad, 
        wrong_pass, bad_login 
    }
}
