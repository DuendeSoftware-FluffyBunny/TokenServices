namespace FluffyBunny4.DotNetCore.Services
{
    public interface IHostStorage
    {
        void AddOrUpdate(string key, object value);
        bool TryGetValue(string key,out object value);
    }
}
