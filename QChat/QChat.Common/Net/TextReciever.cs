﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QChat.Common;

namespace QChat.Common.Net
{
    public class TextReciever
    {
        public Content GetContent(MessageHeader header, Connection connection)
        {
            var buffer = new byte[header.Length];

            connection.Read(buffer, 0, header.Length);

            return Content.Wrap(buffer);
        }

        public async Task<Content> GetContentAsync(MessageHeader header, Connection connection)
        {
            var buffer = new byte[header.Length];

            await connection.ReadAsync(buffer, 0, header.Length);

            return Content.Wrap(buffer);
        }
    }
}
