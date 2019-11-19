using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.CLient
{
    static class StaticProvider
    {
        private static Dictionary<Type, object> _objects = new Dictionary<Type, object>();

        public static bool TryGetInstanceOf<T>(out T instance)
        {
            var success = _objects.TryGetValue(typeof(T), out var result);
            instance = (T)result;
            return success;
        }
        public static T GetInstanceOf<T>()
        {
            return (T)_objects[typeof(T)];
        }

        public static bool IsRegistered<T>()
        {
            return _objects.ContainsKey(typeof(T));
        }

        public static bool TryRegisterInstanceOf<T>(T instance) 
        {
            if (instance == null) throw new ArgumentNullException();

            var instanceType = typeof(T);

            if (_objects.ContainsKey(instanceType))
                return false;

            _objects.Add(instanceType, instance);
            return true;
        }
        public static void RegisterInstanceOf<T>(T instance)
        {
            if (instance == null) throw new ArgumentNullException();

            _objects.Add(typeof(T), instance);
        }

        public static bool TryUnregisterInstanceOf<T>()
        {
            return _objects.Remove(typeof(T));
        }
        public static void UnregisterInstanseOf<T>()
        {
            var instanceType = typeof(T);

            if (!_objects.Remove(instanceType))
                throw new ArgumentException($"Couldn't get instance of {instanceType.FullName}");
        }
    }
}
