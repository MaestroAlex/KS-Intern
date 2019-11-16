using System;
using System.Threading.Tasks;


namespace QChat.Common.Net
{
    public interface IConnection : IConnectionStream, IDisposable
    {
        bool Connected { get; }
        ulong Id { get; }
    }
}