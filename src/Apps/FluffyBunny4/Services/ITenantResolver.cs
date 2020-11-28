using System.Threading.Tasks;

namespace FluffyBunny4.Services
{
    public interface ITenantResolver
    {
        Task<ISignatureProvider> GetSignatureProviderAsync(string tenantId);
        Task<IJwksDiscovery> GetJwksDisoveryAsync(string tenantId);
    }
}
