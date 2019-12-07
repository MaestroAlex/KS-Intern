namespace ChatMVVM.ViewModels
{
    class Chat
    {
        //static public int CurentChatID;
        private string _Name;
        private string _History = "";
        private int _ID;

        public string Name
        {
            get => _Name;

            set
            {
                _Name = value;
            }
        }
        public string History
        {
            get => _History;

            set
            {
                _History = value;
            }
        }
        public int ID => _ID;

        public Chat(int chatID, string chatName, string chatHistory = "")
        {
            _ID = chatID;
            _Name = chatName;
            _History = chatHistory;
        }

        
    }
}
