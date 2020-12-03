using System.Collections.Generic;
using System.Threading.Tasks;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using FluffyBunny4.Models;
using FluffyBunny4.Stores;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Stores
{
    class EntityFrameworkTenantStore : ITenantStore
    {
        
        private IAdminServices _adminServices;
        private ILogger<EntityFrameworkTenantStore> _logger;

        public EntityFrameworkTenantStore(
            IAdminServices adminServices,
            ILogger<EntityFrameworkTenantStore> logger)
        {
            _adminServices = adminServices;
            _logger = logger;
        }

        public async Task<TenantHandle> FindTenantByIdAsync(string tenantId)
        {
            var entity = await _adminServices.GetTenantByNameAsync(tenantId);
            if (entity == null)
            {
                return null;
            }
            return new TenantHandle
            {
                Enabled = entity.Enabled,
                TenantId = entity.Name,
                Name = entity.Name,
                Properties = new Dictionary<string, string>
                {
                    {"cosmos:operational:databaseName",$"{entity.Name}-database" },
                    {"cosmos:operational:containerName",$"{entity.Name}-operational"}
                }
            };
        }
    }
}