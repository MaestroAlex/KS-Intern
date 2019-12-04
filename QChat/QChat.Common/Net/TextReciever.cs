using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common;

namespace QChat.Common.Net
{
    public class TextReciever : IContentReciever
    {
        public Content GetContent(MessageHeader header, IConnectionStream connection)
        {
            var buffer = new byte[header.Length];

            connection.ReadAll(buffer, 0, header.Length);

            return Content.Wrap(buffer);
        }

        public async Task<Content> GetContentAsync(MessageHeader header, IConnectionStream connection)
        {
            var buffer = new byte[header.Length];

            await connection.ReadAllAsync(buffer, 0, header.Length);

            return Content.Wrap(buffer);
        }
    }
}
