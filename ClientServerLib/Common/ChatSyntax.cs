using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerLib.Common
{
    public class ChatSyntax
    {
        #region Разделители для сообщений
        public static string MessageDiv = "<msg:div>";
        public static string ImageDiv = "<msg:img>";
        #endregion

        #region Команды для сервера
        public static string LoginCmd = "/login";
        public static string HelpCmd = "/help";
        public static string UserListCmd = "/list";
        public static string RegCmd = "/reg";
        public static string InviteToRoomCmd = "/invite";
        #endregion

        #region Команды для клиента
        public static string SignedInSignalCmd = "/signed_in";
        public static string ChatHistoryCmd = "/history";
        #endregion

        #region Общие команды
        public static string EnterRoomCmd = "/enterroom";
        public static string CreateRoomCmd = "/room";
        #endregion

        #region Директории и файлы
        public static string ResourcesDir = "ChatData";
        public static string SettingsFile = "Settings.ini";
        #endregion

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
