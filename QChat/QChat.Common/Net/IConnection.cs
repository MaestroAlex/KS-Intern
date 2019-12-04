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

        void Close();

        bool WaitForData(int miliseconds);

        bool Connected { get; }
        int Id { get; }
        int DataAvailable { get; }
    }
}