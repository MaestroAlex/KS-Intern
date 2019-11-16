using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Common.Net
{
    public interface IContentSender
    {
        bool SendContent(IConnectionStream connection, Content content);
        Task<bool> SendContentAsync(IConnectionStream connection, Content content);
    }
}
