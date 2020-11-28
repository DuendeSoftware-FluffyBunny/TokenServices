using System.Text;
using IdentityModel.Client;

namespace FluffyBunny4.Services
{
    public interface IDiscoveryCacheAccessor
    {
        IDiscoveryCache GetCache(string name);
    }
}
