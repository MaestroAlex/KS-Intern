using ChatClient.Interface;
using ChatClient.Models;
using ChatClient.Views.ChatView;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TransitPackage;

namespace ChatClient.ViewModel.ChatViewModel
{
    public class ChatsViewModel : ViewModelBase
    {
        private IFrameNavigationService navigation;

        private HamburgerMenuItemCollection channels;
        public HamburgerMenuItemCollection Channels
        {
            get => channels;
            private set => Set(ref channels, value);
        }

        public ChatsViewModel(IFrameNavigationService navigation)
        {
            this.navigation = navigation;
            Channels = new HamburgerMenuItemCollection();
            ChatsLoaded = new RelayCommand(ChatsLoadedCommandExecute);
            ChatsUnloaded = new RelayCommand(ChatsUnloadedCommandExecute);
        }

        private void ConnectedChannels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Channel newChannel = (Channel)e.NewItems[0];

                HamburgerMenuIconItem item = new HamburgerMenuIconItem() 
                { 
                    Label = newChannel.Name,
                    Tag = new CurrentChatView()
                    {
                        DataContext = new CurrentChatViewModel(newChannel)
                    }
                };

                if (newChannel.Type == ChannelType.user)
                    item.Icon = "UserSolid";
                else if (newChannel.Type == ChannelType.public_open)
                    item.Icon = "UserFriendsSolid";
                else if (newChannel.Type == ChannelType.public_closed)
                    item.Icon = "UserLockSolid";

                Channels.Add(item);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                HamburgerMenuItem removedUser = Channels.FirstOrDefault(cur => ((Channel)e.OldItems[0]).Name == cur.Label);
                Channels.Remove(removedUser);
            }

            //CheckForNoUsersPage();
        }

        private void SetNewViewChannels()
        {
            foreach (var item in MainModel.ConnectedChannels)
            {
                HamburgerMenuIconItem newChannel = new HamburgerMenuIconItem() { Label = item.Name, };

                if (item.IsEntered)
                    newChannel.Tag = new CurrentChatView() { DataContext = new CurrentChatViewModel(item) };
                else
                    newChannel.Tag = new NotEnteredChatView() { DataContext = new NotEnteredChatViewModel(item) };

                if (item.Type == ChannelType.user)
                    newChannel.Icon = "UserSolid";
                else if (item.Type == ChannelType.public_open)
                    newChannel.Icon = "UserFriendsSolid";
                else if (item.Type == ChannelType.public_closed)
                    newChannel.Icon = "UserLockSolid";

                if (!Channels.Where((cur) => cur.Label == newChannel.Label).Any())
                    Channels.Add(newChannel);
            }
        }

        private async Task GetChannelsFromServer()
        {
            await Task.Run(() => MainModel.GetUsers());
        }

        private void CheckForNoUsersPage()
        {
            if (MainModel.ConnectedChannels.Count != 0)
                navigation.NavigateTo("ChatsPage");
            else
                navigation.NavigateTo("NoUsersPage");
        }

        private void CreateNewRoomButton()
        {
            HamburgerMenuIconItem roomCreation = new HamburgerMenuIconItem()
            {
                Label = "New room",
                Icon = "UserPlusSolid",
                Tag = new NewRoomView()
                {
                    DataContext = new NewRoomViewModel()
                }
            };
            Channels.Add(roomCreation);
        }

        #region Commands

        #region ChatsLoadedCommand
        public RelayCommand ChatsLoaded { get; private set; }

        private bool channelCollectionSigned = false;
        public async void ChatsLoadedCommandExecute()
        {
            await GetChannelsFromServer();
            //CheckForNoUsersPage();
            CreateNewRoomButton();
            SetNewViewChannels();

            if (!channelCollectionSigned)
            {
                MainModel.ConnectedChannels.CollectionChanged += ConnectedChannels_CollectionChanged;
                channelCollectionSigned = true;
            }

            await Task.Run(() => MainModel.Client.GetAllHistoryActionRequest());
        }
        #endregion

        #region ChatsUnloadedCommand
        public RelayCommand ChatsUnloaded { get; private set; }

        public void ChatsUnloadedCommandExecute()
        {
            Channels.Clear();
        }
        #endregion

        #endregion
    }
}
