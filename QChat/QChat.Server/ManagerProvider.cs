using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Server
{
    class ManagerProvider : IManagerProvider
    {
        private Dictionary<Type, object> _managers;


        public ManagerProvider()
        {
            _managers = new Dictionary<Type, object>();
        }

        public void Register<T>(T manager) where T : class
        {
            _managers.Add(manager.GetType(), manager);
        }

        public T Get<T>() where T : class
        {
            _managers.TryGetValue(typeof(T), out var result);
            return result as T;
        }
    }
}
