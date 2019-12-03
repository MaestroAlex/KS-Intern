using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerLib.Common
{
    public class ChatSyntax
    {
        public static string UserMessageDiv = "<msg:div>";
        public static string LoginCmd = "/login";
        public static string CreateRoomCmd = "/room";
        public static string HelpCmd = "/help";
        public static string UserListCmd = "/list";
        public static string RegCmd = "/reg";
        public static string EnterRoomCmd = "/enterroom";
        public static string InviteToRoomCmd = "/invite";
        public static string SignedInSignalCmd = "/signed_in";
        public static string ChatHistoryCmd = "/history";
        //public static string[] MessageCommands = new string[] { "/login", "/room", "/help", "/list", "/w", "/reg", "/enter", "/invite" };

        public static string HelpString
        {
            get
            {
                return "Help:\n" +
                    "Type text in textbox and click \"Send\" button to send your message.\n" +
                    "Type special command to perform an action.\n" +
                    "/list - get nicknames of all users in current chat room.\n" +
                    "/room [room name] - create chat room.\n" +
                    "/invite [room name] [username1] (username2 ...) - invite users in chat room.\n" +
                    "Example: /invite MyChat PavlikMoroz MarinaShhh\n" +
                    "/help - get this help message again.";
            }
        }
    }
}
