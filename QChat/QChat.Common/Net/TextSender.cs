using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Common.Net
{
    public class TextSender : ISender
    {
        public bool SendContent(Connection connection, Content content)
        {
            try
            {
                connection.Write(content.AsBytes(), 0, content.Length);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<bool> SendContentAsync(Connection connection, Content content)
        {
            try
            {
                await connection.WriteAsync(content.AsBytes(), 0, content.Length);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
