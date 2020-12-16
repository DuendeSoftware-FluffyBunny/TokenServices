using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FluffyBunny4.DotNetCore.Services
{
    internal class ThreadSafeScopedStorage : IScopedStorage
    {
        ConcurrentDictionary<string, object> Storage { get; }
        public ThreadSafeScopedStorage()
        {
            Storage = new ConcurrentDictionary<string, object>();
        }
        public void AddOrUpdate(string key, object value)
        {
            Storage.AddOrUpdate(key, value, (key, oldValue) => value);
        }
        public bool TryGetValue(string key, out object value)
        {
           
            return Storage.TryGetValue(key, out value);
        }

        public bool TryRemove(string key, out object value)
        {
            return Storage.TryRemove(key, out value);
        }
    }
}