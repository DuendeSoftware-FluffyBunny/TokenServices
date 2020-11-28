using FluffyBunny4.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluffyBunny4.Stores
{
    public class InMemoryTenantStore  : ITenantStore
    {
        private IEnumerable<TenantHandle> _tenants;

        public InMemoryTenantStore(
            IEnumerable<TenantHandle> tenants
            )
        {
             _tenants = tenants;
        }


        /// <summary>
        /// Finds a tenant by id
        /// </summary>
        /// <param name="tenantId">The tenant id</param>
        /// <returns>
        /// The tenant
        /// </returns>
        public Task<TenantHandle> FindTenantByIdAsync(string tenantId)
        {
            var query =
                 from tenant in _tenants
                 where tenant.TenantId == tenantId
                 select tenant;
            return Task.FromResult(query.SingleOrDefault());
        }
    }
}
