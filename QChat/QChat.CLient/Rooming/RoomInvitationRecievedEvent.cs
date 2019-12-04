using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common;

namespace QChat.CLient.Rooming
{
    delegate void RoomInvitationRecievedEventHandler(object sender, RoomInvitationRecievedEventArgs args);

    class RoomInvitationRecievedEventArgs
    {
        public RoomInvitationNotification InvitationInfo;

        public RoomInvitationRecievedEventArgs(RoomInvitationNotification invitationInfo)
        {
            InvitationInfo = invitationInfo;
        }
    }
}
