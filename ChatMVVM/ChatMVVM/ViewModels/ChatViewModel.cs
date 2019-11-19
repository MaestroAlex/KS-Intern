using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatMVVM.Annotations;
using ChatMVVM.Models;
using ChatMVVM.ViewModels.Commands;

namespace ChatMVVM.ViewModels
{
    class ChatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        NetworkStream serverStream = default(NetworkStream);
        string readData = null;
        string _History = "";

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public ICommand SendMsg => new DelegateCommand(SendMessage, (obj) => !string.IsNullOrEmpty(_Message));

        private string _Message;

        public string UserName
        {
            get => AuthViewModel._User.Name;

        }


        public string Message
        {
            get
            {
                return _Message;
            }

            set
            {
                _Message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public string History
        {
            get
            {
                return _History;
            }
            set
            {
                _History = value;
                OnPropertyChanged(nameof(History));
            }
        }

        public ChatViewModel()
        {
            Task.Factory.StartNew(ReceiveMessage);
        }

        public void SendMessage(object obj)
        {
            AuthViewModel._User.Socket.Send(Encoding.Unicode.GetBytes(Message + "$"));
            History += DateTime.Now.ToShortTimeString() + " " + AuthViewModel._User.Name + " : " + Message + '\n';
            Message = "";
        }


        private void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    if (AuthViewModel._User.Socket.Connected)
                    {
                        var bytes = AuthViewModel._User.Socket.Available;
                        byte[] buffer = new byte[256];
                        string data = null;
                        AuthViewModel._User.Socket.Receive(buffer);

                        data = Encoding.Unicode.GetString(buffer);
                        data = data.Substring(0, data.LastIndexOf("$"));
                        data += '\n';
                        History += "->"+DateTime.Now.ToShortTimeString() + " | " + data;
                        //var message = DateTime.Now.ToString() + " | " + clientName + " : " + data;
                        //Console.WriteLine(DateTime.Now.ToString() + " | " + clientName + " : " + data);
                        // Program.Broadcast(message, clientName, true);
                    }
                    else
                    {
                        AuthViewModel._User.Socket.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }


    }
}
