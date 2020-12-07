using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using FluffyBunny4.Services;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Stores
{
    class EntityFrameworkResourceStore : IResourceStore
    {
        private IScopedTenantRequestContext _scopedTenantRequestContext;
        private IAdminServices _adminServices;
        private IEntityFrameworkMapperAccessor _entityFrameworkMapperAccessor;
        private ILogger<EntityFrameworkResourceStore> _logger;

        public EntityFrameworkResourceStore(
            IScopedTenantRequestContext scopedTenantRequestContext,
            IAdminServices adminServices,
            IEntityFrameworkMapperAccessor entityFrameworkMapperAccessor,
            ILogger<EntityFrameworkResourceStore> logger)
        {
            _scopedTenantRequestContext = scopedTenantRequestContext;
            _adminServices = adminServices;
            _entityFrameworkMapperAccessor = entityFrameworkMapperAccessor;
            _logger = logger;
        }

        public async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
        {
            var tenantName = _scopedTenantRequestContext.TenantId;
            var apiResources = await _adminServices.GetAllApiResourcesAsync(tenantName);

            var query = from item in apiResources
                where apiResourceNames.Contains(item.Name)
                let c = _entityFrameworkMapperAccessor.MapperOneToOne.Map<ApiResource>(item)
                select c;
            return query;
        }

        public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            var tenantName = _scopedTenantRequestContext.TenantId;
            var apiResourceEntities = await _adminServices.GetAllApiResourcesAsync(tenantName);
            var finalList = new List<ApiResource>();

            foreach (var apiResourceEntity in apiResourceEntities)
            {

                var queryARS = from item in apiResourceEntity.Scopes
                    select item.Scope;
                var listArs = queryARS.ToList();
                var duplicates = scopeNames.Intersect(listArs);
                if (duplicates.Any())
                {
                    finalList.Add(_entityFrameworkMapperAccessor.MapperOneToOne.Map<ApiResource>(apiResourceEntity));
                }
            }

            return finalList;
        }

        public async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            var tenantName = _scopedTenantRequestContext.TenantId;
            var apiResourceScopes = await _adminServices.GetAllApiResourceScopesAsync(tenantName, ClientScopesSortType.NameDesc);

            var apiResourceScopeNames = (from item in apiResourceScopes
                                         select item.Scope).ToList();


            var finalList = new List<ApiScope>();

            var duplicates = scopeNames.Intersect(apiResourceScopeNames);
            var query = from item in duplicates
                let c = new ApiScope(item)
                select c;
            return query;

 
        }

        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            return Task.FromResult(Enumerable.Empty<IdentityResource>());
        }

        public async Task<Resources> GetAllResourcesAsync()
        {
            var tenantName = _scopedTenantRequestContext.TenantId;
            var apiResources = await _adminServices.GetAllApiResourcesAsync(tenantName);
            var queryApiResources = from item in apiResources
                let c = _entityFrameworkMapperAccessor.MapperOneToOne.Map<ApiResource>(item)
                select c;
            var result = new Resources(
                Enumerable.Empty<IdentityResource>(),
                queryApiResources,
                Enumerable.Empty<ApiScope>());

            return result;
        }
    }
}