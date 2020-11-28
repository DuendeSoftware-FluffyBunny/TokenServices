using System.Collections.Generic;
using System.Threading.Tasks;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Entities;
using FluffyBunny4.DotNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Services
{
    internal class AdminServices : IAdminServices
    {
        private IMainEntityCoreContext _mainEntityCoreContext;
        private ITenantAwareConfigurationDbContextAccessor _tenantAwareConfigurationDbContextAccessor;
        private ILogger<AdminServices> _logger;

        public AdminServices(
            IMainEntityCoreContext mainEntityCoreContext,
            ITenantAwareConfigurationDbContextAccessor tenantAwareConfigurationDbContextAccessor,
            ILogger<AdminServices> logger)
        {
            _mainEntityCoreContext = mainEntityCoreContext;
            _tenantAwareConfigurationDbContextAccessor = tenantAwareConfigurationDbContextAccessor;
            _logger = logger;
        }

        public async Task CreateTenantAsync(string name)
        {
            Guard.ArgumentNotNullOrEmpty(nameof(name),name);

            // ensure db created
            await _mainEntityCoreContext.DbContext.Database.EnsureCreatedAsync();
            await _mainEntityCoreContext.DbContext.Database.MigrateAsync();
            var tenantInDb = await _mainEntityCoreContext.Tenants.FirstOrDefaultAsync(x => x.Name == name);
            if (tenantInDb == null)
            {
                var tenantContext =
                    _tenantAwareConfigurationDbContextAccessor.GetTenantAwareConfigurationDbContext(name);
                var tenant = new Tenant
                {
                    Name = name,
                    Enabled = false
                };
                await _mainEntityCoreContext.Tenants.AddAsync(tenant);
                await _mainEntityCoreContext.SaveChangesAsync();

                // ensure tenant database is created
                await tenantContext.DbContext.Database.EnsureCreatedAsync();
                await tenantContext.DbContext.Database.MigrateAsync();
            }
        }

        public async Task<IEnumerable<Tenant>> GetAllTenantsAsync()
        {
            return await _mainEntityCoreContext
                .Tenants
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Tenant> GetTenantByNameAsync(string tenantId)
        {
            return await _mainEntityCoreContext
                .Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == tenantId);
        }

        public async Task UpdateTenantAsync(Tenant tenant)
        {
            var tenantInDb = await _mainEntityCoreContext
                .Tenants
                .FirstOrDefaultAsync(x => x.Name == tenant.Name);
            if (tenantInDb != null)
            {
                // no match, so no 
                tenantInDb.Enabled = tenant.Enabled;
                await _mainEntityCoreContext.SaveChangesAsync();
            }
        }
    }
}