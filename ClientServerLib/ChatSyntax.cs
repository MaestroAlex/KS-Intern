using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerLib
{
    class ChatSyntax
    {
        public static string LoginMessage = "$l";
        //name, room, help
        public static string[] MessageCommands = new string[] { "/login", "/room", "/help", "/list", "/w", "/reg", "/enter", "/invite" };
        public static string HelpStringRu { get { return "Помощь:\n" +
                    "Напишите текст и нажмите кнопку \"Отправить\" для отправки сообщения.\n" +
                    "Введите специальную команду и отправьте ее, чтобы выполнить определенное действие.\n" +
                    "/list - получить список пользователей в чате.\n" +
                    "/room [Имя комнаты] - создать комнату.\n" +
                    "/h для получения этого сообщения."; } }

        public static string HelpString
        {
            get
            {
                return "Help:\n" +
                    "Type text in textbox and click \"Send\" button to send your message.\n" +
                    "Type special command to perform an action.\n" +
                    "/list - get all clients nicknames in chat.\n" +
                    "/room [room name] - create chat room.\n" +
                    "/invite [user name] [room name] - invite user in chat room.\n" +
                    "/w [user name] [text] - whisper message to certain client.\n" +
                    "/help - get this help message again.";
            }
        }
    }
}
