namespace QChat.Server
{
    interface IManagerProvider
    {
        T Get<T>() where T : class;
        void Register<T>(T manager) where T : class;
    }
}