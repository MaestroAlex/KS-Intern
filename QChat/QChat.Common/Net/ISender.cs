using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Common.Net
{
    public interface ISender
    {
        bool SendContent(Connection connection, Content content);
        Task<bool> SendContentAsync(Connection connection, Content content);
    }
}
