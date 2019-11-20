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
        private static Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();

        public static bool TryGetInstanceOf<T>(out T instance)
        {
            var type = typeof(T);

            if(_objects.TryGetValue(type, out var result))
            {
                instance = (T)result;
                return true;
            }
           
            if(_factories.TryGetValue(type, out var factoryMethod))
            {
                instance = (T)factoryMethod();
                return true;
            }

            instance = (T)result;
            _objects.Add(type, instance);
            return false;
        }
        public static T GetInstanceOf<T>()
        {
            if (_objects.TryGetValue(typeof(T), out var instance)) return (T)instance;

            var factoryMethod = _factories[typeof(T)];
            instance = factoryMethod();
            _objects.Add(typeof(T), instance);
            return (T)instance;
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

        public static void RegisterFactory<T>(Func<object> factoryMethod)
        {
            _factories.Add(typeof(T), factoryMethod);
        }
        public static bool TryRegisterFactory<T>(Func<object> factoryMethod)
        {
            if (_factories.ContainsKey(typeof(T))) return false;

            _factories.Add(typeof(T), factoryMethod);
            return true;
        }
    }
}
