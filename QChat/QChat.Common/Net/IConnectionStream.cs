using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Common.Net
{
    public interface IConnectionStream
    {
        int Read(byte[] buffer, int offset, int length);
        Task<int> ReadAsync(byte[] buffer, int offset, int length);

        bool Write(byte[] buffer, int offset, int length);
        Task<bool> WriteAsync(byte[] buffer, int offset, int length);
    }
}
