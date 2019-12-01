using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace ChatMVVM.ViewModels
{
    class MainWindowModel : INotifyPropertyChanged
    {
        public MainWindowModel()
        {
            currentPage = new Views.AuthPage();
        }

        private static MainWindowModel Instance; 
        static public Page chatPage;

        private Page _currentPage;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(prop));
        }

        public Page currentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(currentPage));
            }
        }

        public static MainWindowModel GetInstance()
        {
            if(Instance == null)
            {
                Instance = new MainWindowModel();
            }
            return Instance;
        }

        public void setChatPage()
        {
            if(chatPage==null)
            {
                chatPage = new Views.ChatPage();
            }
            currentPage = chatPage;
            Console.WriteLine("ChatView");
        }

    }
}
