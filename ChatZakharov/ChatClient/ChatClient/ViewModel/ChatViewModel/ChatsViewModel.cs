using ChatClient.Interface;
using ChatClient.Models;
using ChatClient.Utility;
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
using System.Windows;
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
            LeaveRoomCommand = new RelayCommand<object>(LeaveRoomCommandExecute);
        }

        private void ConnectedChannels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Channel newChannel = (Channel)e.NewItems[0];

                HamburgerMenuIconLeaveItem item = new HamburgerMenuIconLeaveItem()
                {
                    Label = newChannel.Name,
                    Icon = DefineIcon(newChannel.Type),
                    LeaveButtonVisibility = DefineLeaveButtonVisibility(newChannel.Type, newChannel.IsEntered),
                    LeaveCommand = LeaveRoomCommand,
                    Tag = new CurrentChatView()
                    {
                        DataContext = new CurrentChatViewModel(newChannel)
                    }
                };

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
                HamburgerMenuIconLeaveItem newChannel = new HamburgerMenuIconLeaveItem() 
                {
                    Label = item.Name,
                    Icon = DefineIcon(item.Type),
                    LeaveButtonVisibility = DefineLeaveButtonVisibility(item.Type, item.IsEntered),
                    LeaveCommand = LeaveRoomCommand
                };

                if (item.IsEntered)
                    newChannel.Tag = new CurrentChatView() { DataContext = new CurrentChatViewModel(item) };
                else
                    newChannel.Tag = new NotEnteredChatView() { DataContext = new NotEnteredChatViewModel(item) };

                if (!Channels.Where((cur) => cur.Label == newChannel.Label).Any())
                    Channels.Add(newChannel);
            }
        }

        private async Task GetChannelsFromServer()
        {
            await Task.Run(() => MainModel.GetChannels());
        }

        private void CreateNewRoomButton()
        {
            HamburgerMenuIconLeaveItem roomCreation = new HamburgerMenuIconLeaveItem()
            {
                Label = "New room",
                Icon = "UserPlusSolid",
                LeaveButtonVisibility = Visibility.Collapsed,
                LeaveCommand = LeaveRoomCommand,
                Tag = new NewRoomView()
                {
                    DataContext = new NewRoomViewModel()
                }
            };
            Channels.Add(roomCreation);
        }

        private string DefineIcon(ChannelType channelType)
        {
            switch (channelType)
            {
                case ChannelType.user:
                    return "UserSolid";
                case ChannelType.public_open:
                    return "UserFriendsSolid";
                case ChannelType.public_closed:
                    return "UserLockSolid";
            }
            throw new ArgumentException("not implemented channelType icon");
        }

        private Visibility DefineLeaveButtonVisibility(ChannelType channelType, bool isEntered)
        {
            if (!isEntered || channelType == ChannelType.user)
                return Visibility.Collapsed;
            else if (channelType == ChannelType.public_open || channelType == ChannelType.public_closed)
                return Visibility.Visible;

            throw new ArgumentException("not implemented channelType icon");
        }

        #region Commands

        #region ChatsLoadedCommand
        public RelayCommand ChatsLoaded { get; private set; }

        private bool channelCollectionSigned = false;
        public async void ChatsLoadedCommandExecute()
        {
            await GetChannelsFromServer();
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


        #region LeaveRoomCommand
        public RelayCommand<object> LeaveRoomCommand { get; private set; }

        public async void LeaveRoomCommandExecute(object room)
        {
            Channel curChannel = MainModel.ConnectedChannels
                .Where(channel => channel.Name == (string)room)
                .First();

            bool res = await Task.Run(() => MainModel.Client.LeaveRoomActionRequest(room));

            if (res)
            {
                curChannel.IsEntered = false;

                HamburgerMenuIconLeaveItem viewChannel =
                    Channels.Where(viewChannel => viewChannel.Label == curChannel.Name).First()
                    as HamburgerMenuIconLeaveItem;

                viewChannel.Tag =
                    new NotEnteredChatView() { DataContext = new NotEnteredChatViewModel(curChannel) };

                viewChannel.LeaveButtonVisibility = 
                    DefineLeaveButtonVisibility(curChannel.Type, curChannel.IsEntered);
            }
        }
        #endregion

        #endregion
    }
}