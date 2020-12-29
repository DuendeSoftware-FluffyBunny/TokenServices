using FluffyBunny.EntityFramework.Context;

namespace Microsoft.EntityFrameworkCore
{
    public interface ITenantAwareConfigurationDbContextAccessor
    {
        ITenantAwareConfigurationDbContext GetTenantAwareConfigurationDbContext(string tenantId);
    }
}