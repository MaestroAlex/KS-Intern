using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.CLient.ViewModels
{
    class ViewModelLocator
    {
        public ViewModelLocator()
        {
            StaticProvider.TryRegisterFactory<AuthorizationVM>(() => new AuthorizationVM());
            StaticProvider.TryRegisterFactory<MainVM>(() => new MainVM());
            StaticProvider.TryRegisterFactory<ChatVM>(() => new ChatVM());
            StaticProvider.TryRegisterFactory<NavigationVM>(() => new NavigationVM());
            StaticProvider.TryRegisterFactory<RegistrationVM>(() => new RegistrationVM());
            StaticProvider.TryRegisterFactory<RoomDialogVM>(() => new RoomDialogVM());
            StaticProvider.TryRegisterFactory<RoomCreationVM>(() => new RoomCreationVM());
            StaticProvider.TryRegisterFactory<RoomJoiningVM>(() => new RoomJoiningVM());
        }

        public NavigationVM NavigationVM => StaticProvider.GetInstanceOf<NavigationVM>();
        public AuthorizationVM AuthorizationVM => StaticProvider.GetInstanceOf<AuthorizationVM>();
        public MainVM MainVM => StaticProvider.GetInstanceOf<MainVM>();
        public RegistrationVM RegistrationVM => StaticProvider.GetInstanceOf<RegistrationVM>();
        public RoomCreationVM RoomCreationVM => StaticProvider.GetInstanceOf<RoomCreationVM>();
        public RoomDialogVM RoomPlusVM => StaticProvider.GetInstanceOf<RoomDialogVM>();
        public RoomJoiningVM RoomJoiningVM => StaticProvider.GetInstanceOf<RoomJoiningVM>();
    }
}
