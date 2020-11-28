using FluffyBunny4.Stores;
using System.Threading.Tasks;

namespace FluffyBunny4.Extensions
{
    public static class ITenantStoreExtensions
    {
        public static async Task<bool> IsTenantValidAsync(this ITenantStore self, string tenantId)
        {
            var tenant = await self.FindTenantByIdAsync(tenantId);
            if (tenant == null || !tenant.Enabled) return false;
            return true;
        }
    }
}
