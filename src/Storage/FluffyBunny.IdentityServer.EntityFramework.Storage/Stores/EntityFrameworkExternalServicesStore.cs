﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using FluffyBunny4.Models;
using FluffyBunny4.Services;
using FluffyBunny4.Stores;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Stores
{
    public class EntityFrameworkExternalServicesStore : IExternalServicesStore
    {
        private ITenantRequestContext _tenantRequestContext;
        private IAdminServices _adminServices;
        private IEntityFrameworkMapperAccessor _entityFrameworkMapperAccessor;
        private ILogger<EntityFrameworkExternalServicesStore> _logger;

        public EntityFrameworkExternalServicesStore(
            ITenantRequestContext tenantRequestContext,
            IAdminServices adminServices,
            IEntityFrameworkMapperAccessor entityFrameworkMapperAccessor,
            ILogger<EntityFrameworkExternalServicesStore> logger)
        {
            _tenantRequestContext = tenantRequestContext;
            _adminServices = adminServices;
            _entityFrameworkMapperAccessor = entityFrameworkMapperAccessor;
            _logger = logger;
        }
        public async Task<ExternalService> GetExternalServiceByNameAsync(string serviceName)
        {
            var tenantId = _tenantRequestContext.TenantId;
            var entity = await _adminServices.GetExternalServiceByNameAsync(tenantId, serviceName);
            var result = _entityFrameworkMapperAccessor.MapperOneToOne.Map<ExternalService>(entity);
            return result;
        }

        public async Task<List<ExternalService>> GetExternalServicesAsync()
        {
            var tenantId = _tenantRequestContext.TenantId;
            var entities = await _adminServices.GetAllExternalServicesAsync(tenantId);
            var result = (from item in entities
                let c = _entityFrameworkMapperAccessor.MapperOneToOne.Map<ExternalService>(item)
                select c).ToList();
            return result;
        }
    }
}