using FluffyBunny4.Models;
using System.Threading.Tasks;

namespace FluffyBunny4.Services
{
    public interface IJwksDiscovery
    {
        Task<JwksDiscoveryDocument> FetchJwkRecordsAsync();
    }
}
