namespace FluffyBunny4.DotNetCore.Services
{
    public interface IScopedStorage
    {
        void AddOrUpdate(string key, object value);
        bool TryGetValue(string key, out object value);
        bool TryRemove(string key, out object value);
    }
}