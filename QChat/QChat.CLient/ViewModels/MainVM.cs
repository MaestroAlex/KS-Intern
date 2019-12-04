using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Collections.ObjectModel;
using QChat.CLient.Messaging;
using QChat.CLient.Services;
using QChat.Common;
using QChat.CLient.Rooming;

namespace QChat.CLient.ViewModels
{
    class MainVM : DependencyObject
    {
        private RoomingService _roomingService;

        private string _currentChatsType = "room";

        public ChatVM ChatVM { get; private set; }

        public ChatListItemVM SelectedChat
        {
            get { return (ChatListItemVM)GetValue(SelectedChatProperty); }
            set
            {                
                SetValue(SelectedChatProperty, value);
            }
        }
        public static readonly DependencyProperty SelectedChatProperty =
            DependencyProperty.Register("SelectedChat", typeof(ChatListItemVM), typeof(MainVM), new PropertyMetadata(null, On_ChatSelection_Changed));

        public Dictionary<string, ObservableCollection<ChatListItemVM>> Chats { get; private set; }        

        public ObservableCollection<ChatListItemVM> CurrentChatList
        {
            get { return (ObservableCollection<ChatListItemVM>)GetValue(CurrentChatListProperty); }
            set { SetValue(CurrentChatListProperty, value); }
        }
        public static readonly DependencyProperty CurrentChatListProperty =
            DependencyProperty.Register("CurrentChatList", typeof(ObservableCollection<ChatListItemVM>), typeof(MainVM), new PropertyMetadata(null));



        public string UserLogin
        {
            get { return (string)GetValue(UserLoginProperty); }
            set { SetValue(UserLoginProperty, value); }
        }
        public static readonly DependencyProperty UserLoginProperty =
            DependencyProperty.Register("UserLogin", typeof(string), typeof(MainVM), new PropertyMetadata(string.Empty));



        public MainVM()
        {
            RegisterServices();
            InitializeChatLists();

            var login = StaticProvider.GetInstanceOf<AuthorizationVM>().Login;

            UserLogin = $"{login}({login.GetHashCode()})";
            ChatVM = StaticProvider.GetInstanceOf<ChatVM>();

            CurrentChatList = Chats[_currentChatsType];
            ChatVM.Type = "room";

            Task.Run(async () => await StaticProvider.GetInstanceOf<ServerListnerService>().ListenToServer());
        }

        private void RegisterServices()
        {
            StaticProvider.RegisterFactory<RoomingService>(() => new RoomingService());
            StaticProvider.RegisterFactory<MessagingService>(() => new MessagingService(new Common.Net.ContentRecieverTable(), new Common.Net.ContentSenderTable(),
                new Common.SenderInfo
                {
                    UserInfo = StaticProvider.GetInstanceOf<AuthorizationService>().AuthorizationInfo.UserInfo
                }));
            StaticProvider.RegisterFactory<MessageHistoryService>(() => new MessageHistoryService());
            StaticProvider.RegisterFactory<ServerListnerService>(() => new ServerListnerService());
            StaticProvider.RegisterFactory<SocialService>(() => new SocialService());
        }
        
        private void InitializeChatLists()
        {
            //Initialize chat collection
            Chats = new Dictionary<string, ObservableCollection<ChatListItemVM>>(3);
            Chats.Add("room", new ObservableCollection<ChatListItemVM>());
            Chats.Add("user", new ObservableCollection<ChatListItemVM>());
            Chats.Add("group", new ObservableCollection<ChatListItemVM>());

            _roomingService = StaticProvider.GetInstanceOf<RoomingService>();

            InitializeRooms();

            _roomingService.RoomInvitationRecieved += OnRoomInvitationRecieved;
        }

        private async Task InitializeRooms()
        {
            await Task.Run(() => _roomingService.GetRoomsFromServer());
            _roomingService.UpdateRoomCollection(Chats["room"]);
        }
        private async Task IntializeGroups()
        {

        }

        private static void On_ChatSelection_Changed(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var mainVM = (d as MainVM);
            var chatVM = mainVM.ChatVM;

            chatVM.Id = mainVM.SelectedChat.Id;              
        }

        public RoomConnectionResult ConnectToRoom(int id)
        {
            try
            {
                return StaticProvider.GetInstanceOf<RoomingService>().ConnectToRoom(id);
            }
            catch(Exception e)
            {
                return RoomConnectionResult.Fail;
            }
        }
        public void DisconnectFromRoom(int id)
        {
            try
            {
                StaticProvider.GetInstanceOf<RoomingService>().Disconnect(id);
            }
            catch(Exception e)
            {

            }
        }

        private void OnRoomInvitationRecieved(object sender, RoomInvitationRecievedEventArgs args)
        {
            var synchronizationInfo = args.InvitationInfo.RoomSynchronizationInfo;

            Chats["room"].Add(
                new ChatListItemVM(synchronizationInfo.Name.Value, synchronizationInfo.Id));
        }
    }
}
