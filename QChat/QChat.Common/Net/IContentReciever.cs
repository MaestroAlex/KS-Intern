using System.Threading.Tasks;

namespace QChat.Common.Net
{
    public interface IContentReciever
    {
        Content GetContent(MessageHeader header, IConnectionStream connection);
        Task<Content> GetContentAsync(MessageHeader header, IConnectionStream connection);
    }
}