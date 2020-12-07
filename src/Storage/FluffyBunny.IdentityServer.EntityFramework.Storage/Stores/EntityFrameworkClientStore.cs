using Duende.IdentityServer.Models;
using System;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer.Stores;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using FluffyBunny4.Models;
using FluffyBunny4.Services;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Stores
{
    class EntityFrameworkClientStore : IClientStore
    {
        private IScopedTenantRequestContext _scopedTenantRequestContext;
        private IAdminServices _adminServices;
        private IEntityFrameworkMapperAccessor _entityFrameworkMapperAccessor;
        private ILogger<EntityFrameworkClientStore> _logger;

        public EntityFrameworkClientStore(
            IScopedTenantRequestContext scopedTenantRequestContext,
            IAdminServices adminServices,
            IEntityFrameworkMapperAccessor entityFrameworkMapperAccessor,
            ILogger<EntityFrameworkClientStore> logger)
        {
            _scopedTenantRequestContext = scopedTenantRequestContext;
            _adminServices = adminServices;
            _entityFrameworkMapperAccessor = entityFrameworkMapperAccessor;
            _logger = logger;
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            
            var tenantName = _scopedTenantRequestContext.TenantId;
            var clientEntity = await _adminServices.GetClientByClientIdAsync(tenantName, clientId);
            var clientExtra = _entityFrameworkMapperAccessor.MapperOneToOne.Map<ClientExtra>(clientEntity);
            return clientExtra;
        }
    }
}
