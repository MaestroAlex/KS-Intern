using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.CLient.Services
{
    class SocialService
    {
        private Dictionary<int, string> _nicknames;

        public SocialService()
        {
            _nicknames = new Dictionary<int, string>();
        }

        public async Task SynzhronizeWithServer()
        {

        }

        public string GetName(int id)
        {
            _nicknames.TryGetValue(id, out var name);
            if (name == null) return id.ToString();
            return name;
        }
    }
}
