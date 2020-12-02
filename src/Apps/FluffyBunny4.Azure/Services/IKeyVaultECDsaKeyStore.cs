using System;
using System.Threading.Tasks;
using FluffyBunny4.Models;
namespace FluffyBunny4.Services
{
    public interface IKeyVaultECDsaKeyStore
    {
        Task<ECDsaKeyCache> FetchCacheAsync();
        Task CreateKeysAsync(DateTime utcStartTime,int count, int expirationDays, int overlappingDays);
    }
}
