using System.Collections.Generic;
using System.Linq;
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
        private IEntityFrameworkMapperAccessor _entityFrameworkMapperAccessor;
        private ILogger<AdminServices> _logger;

        public AdminServices(
            IMainEntityCoreContext mainEntityCoreContext,
            ITenantAwareConfigurationDbContextAccessor tenantAwareConfigurationDbContextAccessor,
            IEntityFrameworkMapperAccessor entityFrameworkMapperAccessor,
            ILogger<AdminServices> logger)
        {
            _mainEntityCoreContext = mainEntityCoreContext;
            _tenantAwareConfigurationDbContextAccessor = tenantAwareConfigurationDbContextAccessor;
            _entityFrameworkMapperAccessor = entityFrameworkMapperAccessor;
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

        ITenantAwareConfigurationDbContext GetTenantContext(string name) =>
            _tenantAwareConfigurationDbContextAccessor.GetTenantAwareConfigurationDbContext(name);
        public async Task DeleteExternalServiceByIdAsync(string tenantName, int id)
        {
            var tenantContext = GetTenantContext(tenantName);
            var entityInDb = await tenantContext.ExternalServices.FirstOrDefaultAsync(e => e.Id == id);
            if (entityInDb != null)
            {
                tenantContext.ExternalServices.Remove(entityInDb);
                await tenantContext.SaveChangesAsync();
            }
        }
        public async Task DeleteExternalServiceByNameAsync(string tenantName, string name)
        {
            var tenantContext = GetTenantContext(tenantName);
            var entityInDb = await tenantContext.ExternalServices.FirstOrDefaultAsync(e => e.Name == name);
            if (entityInDb != null)
            {
                tenantContext.ExternalServices.Remove(entityInDb);
                await tenantContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Tenant>> GetAllTenantsAsync()
        {
            return await _mainEntityCoreContext
                .Tenants
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ExternalService> GetExternalServiceByIdAsync(string tenantName, int id)
        {
            var tenantContext = GetTenantContext(tenantName);
            var entityInDb = await tenantContext.ExternalServices
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
            return entityInDb;
        }

        public async Task<ExternalService> GetExternalServiceByNameAsync(string tenantName, string name)
        {
            var tenantContext = GetTenantContext(tenantName);
            var entityInDb = await tenantContext.ExternalServices
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Name == name);
            return entityInDb;
        }

        public async Task<Tenant> GetTenantByNameAsync(string tenantId)
        {
            return await _mainEntityCoreContext
                .Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == tenantId);
        }

        public async Task<PaginatedList<ExternalService>> PageExternalServicesAsync(
            string tenantName, int pageNumber, int pageSize, ExternalServiceSortType sortType)
        {
            var tenantContext = GetTenantContext(tenantName);

            var entities = from t in tenantContext.ExternalServices
                select t;
            switch (sortType)
            {
                case ExternalServiceSortType.NameDesc:
                    entities = entities.OrderByDescending(t => t.Name);
                    break;
                case ExternalServiceSortType.NameAsc:
                    entities = entities.OrderBy(t => t.Name);
                    break;
                case ExternalServiceSortType.EnabledDesc:
                    entities = entities.OrderByDescending(t => t.Enabled);
                    break;
                case ExternalServiceSortType.EnabledAsc:
                    entities = entities.OrderBy(t => t.Enabled);
                    break;
            }

            var paginatedList = await PaginatedList<ExternalService>
                .CreateAsync(entities.AsNoTracking(), pageNumber, pageSize);
            return paginatedList;
        }

        public async Task<PaginatedList<Tenant>> PageTenantsAsync(int pageNumber, int pageSize, TenantSortType sortType)
        {
            var entities = from t in _mainEntityCoreContext.Tenants
                select t;
            switch (sortType)
            {
                case TenantSortType.NameDesc:
                    entities = entities.OrderByDescending(t => t.Name);
                    break;
                case TenantSortType.NameAsc:
                    entities = entities.OrderBy(t => t.Name);
                    break;
                case TenantSortType.EnabledDesc:
                    entities = entities.OrderByDescending(t => t.Enabled);
                    break;
                case TenantSortType.EnabledAsc:
                    entities = entities.OrderBy(t => t.Enabled);
                    break;
            }

            var paginatedList = await PaginatedList<Tenant>
                .CreateAsync(entities.AsNoTracking(), pageNumber, pageSize);
            return paginatedList;
        }

        public async Task UpdateTenantAsync(Tenant tenant)
        {
            var entityInDb = await _mainEntityCoreContext
                .Tenants
                .FirstOrDefaultAsync(x => x.Name == tenant.Name);
            if (entityInDb != null)
            {
                // no match, so no 
                entityInDb.Enabled = tenant.Enabled;
                await _mainEntityCoreContext.SaveChangesAsync();
            }
        }

        public async Task UpsertExternalServiceAsync(string tenantName, ExternalService entity)
        {
            var tenantContext = GetTenantContext(tenantName);
            var entityInDb = await tenantContext
                .ExternalServices
                .FirstOrDefaultAsync(x => x.Name == entity.Name);
            if (entityInDb != null)
            {
                entityInDb.Enabled = entity.Enabled;
                entityInDb.Authority = entity.Authority;
                entityInDb.Description = entity.Description;
            }
            else
            {
                var newEntity = _entityFrameworkMapperAccessor.MapperIgnoreBase.Map<ExternalService>(entity);
                tenantContext.ExternalServices.Add(newEntity);
            }
            await tenantContext.SaveChangesAsync();
        }

     
    }
}