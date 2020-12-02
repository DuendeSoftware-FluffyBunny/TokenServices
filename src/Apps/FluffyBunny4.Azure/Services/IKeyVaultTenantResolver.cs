using System.Threading.Tasks;

namespace FluffyBunny4.Services
{
    public interface IKeyVaultTenantResolver: ITenantResolver
    {
        Task<IKeyVaultECDsaKeyStore> GetKeyVaultECDsaKeyStoreAsync(string tenantId);
    }
}
