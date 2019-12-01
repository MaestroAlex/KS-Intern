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

        public HamburgerMenu ViewChannelsControl { get; set; }

        private HamburgerMenuItemCollection viewChannels;
        public HamburgerMenuItemCollection ViewChannels
        {
            get => viewChannels;
            private set => Set(ref viewChannels, value);
        }

        public ChatsViewModel(IFrameNavigationService navigation)
        {
            this.navigation = navigation;
            ViewChannels = new HamburgerMenuItemCollection();
            ChatsLoaded = new RelayCommand(ChatsLoadedCommandExecute);
            ChatsUnloaded = new RelayCommand(ChatsUnloadedCommandExecute);
            LeaveRoomCommand = new RelayCommand<object>(LeaveRoomCommandExecute);
        }

        private void ConnectedChannels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Channel newChannel = (Channel)e.NewItems[0];

                HamburgerMenuIconLeaveItem newViewChannel = new HamburgerMenuIconLeaveItem()
                {
                    Label = newChannel.Name,
                    Icon = DefineIcon(newChannel.Type),
                    LeaveButtonVisibility = DefineLeaveButtonVisibility(newChannel.Type, newChannel.IsEntered),
                    LeaveCommand = LeaveRoomCommand,
                };

                if (newChannel.Type == ChannelType.user || newChannel.IsEntered)
                    newViewChannel.Tag = new CurrentChatView() { DataContext = new CurrentChatViewModel(newChannel) };
                else
                    newViewChannel.Tag = new NotEnteredChatView() { DataContext = new NotEnteredChatViewModel(newChannel) };

                ViewChannels.Add(newViewChannel);

                if (newChannel.IsEntered)
                {
                    ViewChannelsControl.SelectedIndex = ViewChannels.IndexOf(newViewChannel);
                    ViewChannelsControl.Content = ViewChannels.Last();
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                HamburgerMenuItem removedChannel = ViewChannels.FirstOrDefault(cur => ((Channel)e.OldItems[0]).Name == cur.Label);
                ViewChannels.Remove(removedChannel);
            }

            //CheckForNoUsersPage();
        }

        private void SetNewViewChannels()
        {
            foreach (var channel in MainModel.ConnectedChannels)
            {
                HamburgerMenuIconLeaveItem newViewChannel = new HamburgerMenuIconLeaveItem()
                {
                    Label = channel.Name,
                    Icon = DefineIcon(channel.Type),
                    LeaveButtonVisibility = DefineLeaveButtonVisibility(channel.Type, channel.IsEntered),
                    LeaveCommand = LeaveRoomCommand
                };

                if (channel.Type == ChannelType.user || channel.IsEntered)
                    newViewChannel.Tag = new CurrentChatView() { DataContext = new CurrentChatViewModel(channel) };
                else
                    newViewChannel.Tag = new NotEnteredChatView() { DataContext = new NotEnteredChatViewModel(channel) };

                // if (!Channels.Where((cur) => cur.Label == newChannel.Label).Any())
                ViewChannels.Add(newViewChannel);
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
            ViewChannels.Add(roomCreation);
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

            if (!channelCollectionSigned)
            {
                CreateNewRoomButton();
                MainModel.ConnectedChannels.CollectionChanged += ConnectedChannels_CollectionChanged;
                channelCollectionSigned = true;
            }
            SetNewViewChannels();

            ViewChannelsControl.Content = ViewChannels.First();

            await Task.Run(() => MainModel.Client.GetAllHistoryActionRequest());
        }
        #endregion

        #region ChatsUnloadedCommand
        public RelayCommand ChatsUnloaded { get; private set; }

        public void ChatsUnloadedCommandExecute()
        {
            for (int i = MainModel.ConnectedChannels.Count - 1; i >= 0; i--)
                MainModel.ConnectedChannels.RemoveAt(i);
        }
        #endregion


        #region LeaveRoomCommand
        public RelayCommand<object> LeaveRoomCommand { get; private set; }

        public async void LeaveRoomCommandExecute(object room)
        {
            Channel curChannel = MainModel.ConnectedChannels
                .Where(channel => channel.Name == (string)room)
                .First();

            bool res = await MainModel.Client.LeaveRoomActionRequest(room);

            if (res)
            {
                curChannel.IsEntered = false;

                HamburgerMenuIconLeaveItem viewChannel =
                    ViewChannels.Where(viewChannel => viewChannel.Label == curChannel.Name).First()
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