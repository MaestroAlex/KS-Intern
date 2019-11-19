using ChatClient.Interface;
using ChatClient.Models;
using ChatClient.Views.ChatView;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
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
            set => Set(ref channels, value);
        }

        public RelayCommand<Task> ChatsLoaded { get; private set; }
        bool firstViewInit = true;
        public ChatsViewModel(IFrameNavigationService navigation)
        {
            this.navigation = navigation;
            Channels = new HamburgerMenuItemCollection();

            ChatsLoaded = new RelayCommand<Task>(async (task) =>
            {
                if (firstViewInit)
                {
                    await GetUsers();
                    CheckForNoUsersPage();
                    CreateNewRoomButton();
                    firstViewInit = false;
                }
                SetNewUsers();
                MainModel.ConnectedChannels.CollectionChanged += ConnectedUsers_CollectionChanged;
                await Task.Run(() => MainModel.Client.GetHistoryActionRequest());
            });
        }

        private void ConnectedUsers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                string newUser = MainModel.ConnectedChannels.Last().Name;
                Channels.Add(new HamburgerMenuItem()
                {
                    Label = newUser,
                    Tag = new CurrentChatView() { DataContext = new CurrentChatViewModel((Channel)e.NewItems[0]) }
                });
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                HamburgerMenuItem removedUser = Channels.FirstOrDefault(cur => ((Channel)e.OldItems[0]).Name == cur.Label);
                //HamburgerMenuItem removedUser = Channels
                //    .FirstOrDefault(elem => !MainModel.ConnectedChannels.(elem.Label));
                Channels.Remove(removedUser);
            }

            CheckForNoUsersPage();
        }

        private void SetNewUsers()
        {
            foreach (var item in MainModel.ConnectedChannels)
            {
                HamburgerMenuItem newUser = new HamburgerMenuItem
                {
                    Label = item.Name,
                    Tag = new CurrentChatView() { DataContext = new CurrentChatViewModel(item) }
                };
                if (!Channels.Where((cur) => cur.Label == newUser.Label).Any())
                    Channels.Add(newUser);
            }
        }

        private async Task GetUsers()
        {
            await Task.Run(() => MainModel.GetUsers());
        }

        private void CheckForNoUsersPage()
        {
            //if (MainModel.ConnectedUsers.Count != 0)
            //    navigation.NavigateTo("ChatsPage");
            //else
            //    navigation.NavigateTo("NoUsersPage");
        }

        private void CreateNewRoomButton()
        {
            HamburgerMenuItem roomCreation = new HamburgerMenuItem()
            {
                Label = "New room",
                Tag = new NewRoomView()
                {
                    DataContext = new NewRoomViewModel(Channels)
                }
            };
            Channels.Add(roomCreation);
        }
    }
}
