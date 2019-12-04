using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace QChat.CLient.ViewModels
{
    class ChatListItemVM : INotifyPropertyChanged
    {
        public string Name { get; private set; }
        public int Id { get; private set; }

        private bool _connected = false;
        public bool Connected
        {
            get => _connected;
            set
            {
                _connected = value;
                OnPropertyChanged("Connected");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ChatListItemVM(string name, int id)
        {
            Name = $"{name}({id})";
            Id = id;
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
