using ChatClient.Interface;
using ChatClient.Models;
using ChatClient.Views.ChatView;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModel.ChatViewModel
{
    public class ChatsViewModel : ViewModelBase
    {
        private IFrameNavigationService navigation;

        public HamburgerMenu HamburgerMenu { get; set; }

        private HamburgerMenuItemCollection users;
        public HamburgerMenuItemCollection Users
        {
            get => users;
            set => Set(ref users, value);
        }

        public RelayCommand<Task> ChatsLoaded { get; private set; }
        bool firstViewInit = true;
        public ChatsViewModel(IFrameNavigationService navigation)
        {
            this.navigation = navigation;
            Users = new HamburgerMenuItemCollection();

            ChatsLoaded = new RelayCommand<Task>(async (task) =>
            {
                if (firstViewInit)
                {
                    await GetUsers();
                    CheckForNoUsersPage();
                    firstViewInit = false;
                }
                SetNewUsers();
                MainModel.ConnectedUsers.CollectionChanged += ConnectedUsers_CollectionChanged;
            });
        }

        private void ConnectedUsers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                string newUser = MainModel.ConnectedUsers.Last();
                Users.Add(new HamburgerMenuItem()
                {
                    Label = newUser,
                    Tag = new CurrentChatView() { DataContext = new CurrentChatViewModel(newUser) }
                });
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                HamburgerMenuItem removedUser = Users
                    .FirstOrDefault(elem => !MainModel.ConnectedUsers.Contains(elem.Label));
                Users.Remove(removedUser);
            }

            CheckForNoUsersPage();
        }

        private void SetNewUsers()
        {
            foreach (var item in MainModel.ConnectedUsers)
            {
                HamburgerMenuItem newUser = new HamburgerMenuItem
                {
                    Label = item,
                    Tag = new CurrentChatView() { DataContext = new CurrentChatViewModel(item) }
                };
                if (!Users.Where((cur) => cur.Label == newUser.Label).Any())
                    Users.Add(newUser);
            }
        }

        private async Task GetUsers()
        {
            await Task.Run(() => MainModel.GetUsers());
        }

        private void CheckForNoUsersPage()
        {
            if (MainModel.ConnectedUsers.Count != 0)
                navigation.NavigateTo("ChatsPage");
            else
                navigation.NavigateTo("NoUsersPage");
        }
    }
}
