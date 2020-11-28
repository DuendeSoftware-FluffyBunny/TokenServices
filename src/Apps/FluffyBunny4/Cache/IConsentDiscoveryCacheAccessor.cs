using FluffyBunny4.Models;
using System.Threading.Tasks;

namespace FluffyBunny4.Cache
{
    public interface IConsentDiscoveryCacheAccessor
    {
        public Task<IConsentDiscoveryCache> GetConsentDiscoveryCacheAsync(string serviceName );
    }
}
