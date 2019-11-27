using System;
using System.Threading.Tasks;


namespace QChat.Common.Net
{
    public interface IConnection : IConnectionStream, IDisposable
    {
        event ConnectionClosedEventHandler ConnectionClosed;

        void LockRead();
        Task LockReadAsync();
        void ReleaseRead();

        void LockWrite();
        Task LockWriteAsync();
        void ReleaseWrite();

        bool Connected { get; }
        int Id { get; }
    }
}