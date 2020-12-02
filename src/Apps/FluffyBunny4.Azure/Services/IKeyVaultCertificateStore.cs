using System.Threading.Tasks;
using FluffyBunny4.Models;
namespace FluffyBunny4.Services
{
    public interface IKeyVaultCertificateStore
    {
        Task<CacheData> FetchCacheDataAsync();
    }
}
