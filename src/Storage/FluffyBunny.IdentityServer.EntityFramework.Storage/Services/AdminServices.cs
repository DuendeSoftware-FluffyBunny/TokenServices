using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
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
        public async Task EnsureTenantDatabaseAsync()
        {
            await _mainEntityCoreContext.DbContext.Database.EnsureCreatedAsync();
            await _mainEntityCoreContext.DbContext.Database.MigrateAsync();
        }
        public async Task CreateTenantAsync(string name)
        {
            Guard.ArgumentNotNullOrEmpty(nameof(name), name);

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

        ITenantAwareConfigurationDbContext GetTenantContext(string name)
        {
            name = name.ToLower();
            return _tenantAwareConfigurationDbContextAccessor.GetTenantAwareConfigurationDbContext(name);
        }
            

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
            name = name.ToLower();
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
            name = name.ToLower();
            var tenantContext = GetTenantContext(tenantName);
            var entityInDb = await tenantContext.ExternalServices
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Name == name);
            return entityInDb;
        }

        public async Task<Tenant> GetTenantByNameAsync(string name)
        {
            name = name.ToLower();
            return await _mainEntityCoreContext
                .Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == name);
        }
        public async Task<List<ExternalService>> GetAllExternalServicesAsync(string tenantName)
        {
            var tenantContext = GetTenantContext(tenantName);
            var entities = from t in tenantContext.ExternalServices
                select t;
            return entities.ToList();
        }
        public async Task<PaginatedList<ExternalService>> PageExternalServicesAsync(
            string tenantName, int pageNumber, int pageSize, ExternalServicesSortType sortType)
        {
            var tenantContext = GetTenantContext(tenantName);

            var entities = from t in tenantContext.ExternalServices
                select t;
            switch (sortType)
            {
                case ExternalServicesSortType.NameDesc:
                    entities = entities.OrderByDescending(t => t.Name);
                    break;
                case ExternalServicesSortType.NameAsc:
                    entities = entities.OrderBy(t => t.Name);
                    break;
                case ExternalServicesSortType.EnabledDesc:
                    entities = entities.OrderByDescending(t => t.Enabled);
                    break;
                case ExternalServicesSortType.EnabledAsc:
                    entities = entities.OrderBy(t => t.Enabled);
                    break;
            }

            var paginatedList = await PaginatedList<ExternalService>
                .CreateAsync(entities.AsNoTracking(), pageNumber, pageSize);
            return paginatedList;
        }

        public async Task<PaginatedList<Tenant>> PageTenantsAsync(int pageNumber, int pageSize,
            TenantsSortType sortType)
        {
            var entities = from t in _mainEntityCoreContext.Tenants
                select t;
            switch (sortType)
            {
                case TenantsSortType.NameDesc:
                    entities = entities.OrderByDescending(t => t.Name);
                    break;
                case TenantsSortType.NameAsc:
                    entities = entities.OrderBy(t => t.Name);
                    break;
                case TenantsSortType.EnabledDesc:
                    entities = entities.OrderByDescending(t => t.Enabled);
                    break;
                case TenantsSortType.EnabledAsc:
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
            entity.Name = entity.Name.ToLower();
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
        public async Task<List<ApiResource>> GetAllApiResourcesAsync(string tenantName)
        {
            var tenantContext = GetTenantContext(tenantName);
            var query = from t in tenantContext.ApiResources
                select t;

            var apiResources = await query
                .Include(x => x.Secrets)
                .Include(x => x.Scopes)
                .Include(x => x.UserClaims)
                .Include(x => x.Properties)
                .AsNoTracking()
                .ToListAsync();

            return apiResources;
        }

        public async Task<List<ApiResourceScope>> GetAllApiResourceScopesAsync(string tenantName, ClientScopesSortType sortType)
        {
            var apiResources = await GetAllApiResourcesAsync(tenantName);

            var query = from item in apiResources
                from scope in item.Scopes
                select scope;
            switch (sortType)
            {
                case ClientScopesSortType.NameDesc:
                    query = query.OrderByDescending(t => t.Scope);
                    break;
                case ClientScopesSortType.NameAsc:
                    query = query.OrderBy(t => t.Scope);
                    break;
                
            }
            return query.ToList();
        }

        public async Task<PaginatedList<ApiResource>> PageApiResourcesAsync(string tenantName, int pageNumber,
            int pageSize,
            ApiResourcesSortType sortType)
        {
            var tenantContext = GetTenantContext(tenantName);

            var entities = from t in tenantContext.ApiResources
                select t;
            switch (sortType)
            {
                case ApiResourcesSortType.NameDesc:
                    entities = entities.OrderByDescending(t => t.Name);
                    break;
                case ApiResourcesSortType.NameAsc:
                    entities = entities.OrderBy(t => t.Name);
                    break;
                case ApiResourcesSortType.EnabledDesc:
                    entities = entities.OrderByDescending(t => t.Enabled);
                    break;
                case ApiResourcesSortType.EnabledAsc:
                    entities = entities.OrderBy(t => t.Enabled);
                    break;
            }

            var paginatedList = await PaginatedList<ApiResource>
                .CreateAsync(entities.AsNoTracking(), pageNumber, pageSize);
            return paginatedList;
        }

        public async Task UpsertApiResourceAsync(string tenantName, ApiResource entity)
        {
            entity.Name = entity.Name.ToLower();
            var tenantContext = GetTenantContext(tenantName);
            var entityInDb = await tenantContext
                .ApiResources
                .FirstOrDefaultAsync(x => x.Name == entity.Name);

            if (entityInDb != null)
            {
                // we can only allow one name
                if (entityInDb.Id == entity.Id)
                {
                    // perfect match, update this.
                    _entityFrameworkMapperAccessor.MapperIgnoreBase.Map(entity, entityInDb);
                    await tenantContext.SaveChangesAsync();
                    return;
                }
                else
                {
                    throw new System.Exception($"Resource already exists with the name={entity.Name}");
                }

            }

            entityInDb = await tenantContext
                .ApiResources
                .FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (entityInDb != null)
            {
                _entityFrameworkMapperAccessor.MapperIgnoreBase.Map(entity, entityInDb);
            }
            else
            {
                var newEntity = _entityFrameworkMapperAccessor.MapperIgnoreBase.Map<ApiResource>(entity);
                tenantContext.ApiResources.Add(newEntity);
            }

            await tenantContext.SaveChangesAsync();
        }

        public async Task<ApiResource> GetApiResourceByNameAsync(string tenantName, string name)
        {
            name = name.ToLower();
            var tenantContext = GetTenantContext(tenantName);

            var query = from item in tenantContext.ApiResources
                where item.Name == name
                select item;
            var apiResourceInDb = await query
                .Include(x => x.Secrets)
                .Include(x => x.Scopes)
                .Include(x => x.UserClaims)
                .Include(x => x.Properties)
                .AsNoTracking()
                .FirstOrDefaultAsync();


            return apiResourceInDb;

        }

        public async Task<ApiResource> GetApiResourceByIdAsync(string tenantName, int id)
        {

            var tenantContext = GetTenantContext(tenantName);
            var query = from item in tenantContext.ApiResources
                        where item.Id == id
                        select item;
            var apiResourceInDb = await query
                .Include(x => x.Secrets)
                .Include(x => x.Scopes)
                .Include(x => x.UserClaims)
                .Include(x => x.Properties)
                .AsNoTracking()
                .FirstOrDefaultAsync();

 
            return apiResourceInDb;
        }

        public async Task DeleteApiResourceByNameAsync(string tenantName, string name)
        {
            name = name.ToLower();
            var tenantContext = GetTenantContext(tenantName);
            var entityInDb = await tenantContext
                .ApiResources
                .FirstOrDefaultAsync(x => x.Name == name);
            if (entityInDb != null)
            {
                tenantContext.ApiResources.Remove(entityInDb);
                await tenantContext.SaveChangesAsync();
            }
        }

        public async Task DeleteApiResourceByIdAsync(string tenantName, int id)
        {
            var tenantContext = GetTenantContext(tenantName);
            var entityInDb = await tenantContext
                .ApiResources
                .FirstOrDefaultAsync(x => x.Id == id);
            if (entityInDb != null)
            {
                tenantContext.ApiResources.Remove(entityInDb);
                await tenantContext.SaveChangesAsync();
            }
        }

        public async Task UpsertApiResourceSecretAsync(string tenantName, int apiResourceId, ApiResourceSecret entity)
        {
            try
            {
                var tenantContext = GetTenantContext(tenantName);

                var query =
                    from apiResource in tenantContext.ApiResources
                    where apiResource.Id == apiResourceId
                    select apiResource;

                var apiResourceInDb = await query
                    .Include(x => x.Secrets)
                    .Include(x => x.Scopes)
                    .Include(x => x.UserClaims)
                    .Include(x => x.Properties)
                    .FirstOrDefaultAsync();

                if (apiResourceInDb == null)
                {
                    throw new System.Exception(
                        $"Api Resource already exists tenant={tenantName}, apiResourceId={apiResourceId}");
                }


                var secretEntity = apiResourceInDb.Secrets.FirstOrDefault(s => s.Id == entity.Id);
                if (secretEntity != null)
                {
                    secretEntity.Expiration = entity.Expiration;
                }
                else
                {
                    apiResourceInDb.Secrets.Add(entity);
                }

                await tenantContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
                throw;

            }


        }


        public async Task<ApiResourceSecret> GetApiResourceSecretByIdAsync(string tenantName, int apiResourceId, int id)
        {
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from apiResource in tenantContext.ApiResources
                where apiResource.Id == apiResourceId
                select apiResource;

            var apiResourceInDb = await query
                .Include(x => x.Secrets)
                .Include(x => x.Scopes)
                .Include(x => x.UserClaims)
                .Include(x => x.Properties)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (apiResourceInDb == null)
            {
                throw new System.Exception(
                    $"Api Resource already exists tenant={tenantName}, apiResourceId={apiResourceId}");
            }

            var result = apiResourceInDb.Secrets.FirstOrDefault(s => s.Id == id);
            return result;
        }



        public async Task DeleteApiResourceBySecretIdAsync(string tenantName, int apiResourceId, int id)
        {
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from apiResource in tenantContext.ApiResources
                where apiResource.Id == apiResourceId
                select apiResource;

            var apiResourceInDb = await query
                .Include(x => x.Secrets)
                .Include(x => x.Scopes)
                .Include(x => x.UserClaims)
                .Include(x => x.Properties)
                .FirstOrDefaultAsync();
            if (apiResourceInDb == null)
            {
                throw new System.Exception(
                    $"Api Resource already exists tenant={tenantName}, apiResourceId={apiResourceId}");
            }

            var result = apiResourceInDb.Secrets.FirstOrDefault(s => s.Id == id);
            if (result != null)
            {
                apiResourceInDb.Secrets.Remove(result);
                await tenantContext.SaveChangesAsync();

            }
        }

        public async Task<IEnumerable<ApiResourceSecret>> GetAllApiResourceSecretsAsync(
            string tenantName, int apiResourceId, SecretsSortType sortType)
        {
            var tenantContext = GetTenantContext(tenantName);

            var query =
                from apiResource in tenantContext.ApiResources
                where apiResource.Id == apiResourceId
                select apiResource;

            if (!query.Any())
            {
                throw new System.Exception(
                    $"Api Resource already exists tenant={tenantName}, apiResourceId={apiResourceId}");
            }

            var api = await query
                .Include(x => x.Secrets)
                .Include(x => x.Scopes)
                .Include(x => x.UserClaims)
                .Include(x => x.Properties)
                .AsNoTracking()
                .FirstOrDefaultAsync();



            var entities = from t in api.Secrets
                select t;
            switch (sortType)
            {
                case SecretsSortType.ExpirationDesc:
                    entities = entities.OrderByDescending(t => t.Expiration);
                    break;
                case SecretsSortType.ExpirationAsc:
                    entities = entities.OrderBy(t => t.Expiration);
                    break;
                case SecretsSortType.DescriptionDesc:
                    entities = entities.OrderByDescending(t => t.Description);
                    break;
                case SecretsSortType.DescriptionAsc:
                    entities = entities.OrderBy(t => t.Description);
                    break;
            }

            return entities;
        }

        public async Task UpsertApiResourceScopeAsync(string tenantName, int apiResourceId, ApiResourceScope entity)
        {
            entity.Scope = entity.Scope.ToLower();
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from apiResource in tenantContext.ApiResources
                where apiResource.Id == apiResourceId
                select apiResource;

            var apiResourceInDb = await query
                .Include(x => x.Scopes)
                .FirstOrDefaultAsync();

            var existingScope = (from item in apiResourceInDb.Scopes
                where item.Scope == entity.Scope
                select item).FirstOrDefault();
            if (existingScope != null)
            {
                return; // already here
            }
            existingScope = (from item in apiResourceInDb.Scopes
                where item.Id == entity.Id
                select item).FirstOrDefault();

            if (existingScope != null)
            {
                // name update
                existingScope.Scope = entity.Scope;
            }
            else
            {
                // brand new
                apiResourceInDb.Scopes.Add(entity);
            }
          
            await tenantContext.SaveChangesAsync();
        }
        
        public async Task<ApiResourceScope> GetApiResourceScopeByNameAsync(string tenantName, int apiResourceId, string name)
        {
            name = name.ToLower();
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from apiResource in tenantContext.ApiResources
                where apiResource.Id == apiResourceId
                select apiResource;

            var apiResourceInDb = await query
                .Include(x => x.Scopes)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var existingScope = (from item in apiResourceInDb.Scopes
                where item.Scope == name
                                 select item).FirstOrDefault();
            return existingScope;
        }
        public async Task<ApiResourceScope> GetApiResourceScopeByIdAsync(string tenantName, int apiResourceId, int id)
        {
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from apiResource in tenantContext.ApiResources
                where apiResource.Id == apiResourceId
                select apiResource;

            var apiResourceInDb = await query
                .Include(x => x.Scopes)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var existingScope = (from item in apiResourceInDb.Scopes
                where item.Id == id
                select item).FirstOrDefault();
            return existingScope;
        }

        public async Task DeleteApiResourceByScopeIdAsync(string tenantName, int apiResourceId, int id)
        {
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from apiResource in tenantContext.ApiResources
                where apiResource.Id == apiResourceId
                select apiResource;

            var apiResourceInDb = await query
                .Include(x => x.Scopes)
                .FirstOrDefaultAsync();

            var existingScope = (from item in apiResourceInDb.Scopes
                where item.Id == id
                select item).FirstOrDefault();
            if (existingScope != null)
            {
                apiResourceInDb.Scopes.Remove(existingScope);
                await tenantContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ApiResourceScope>> GetAllApiResourceScopesAsync(string tenantName, int apiResourceId,
            ApiResourceScopesSortType sortType = ApiResourceScopesSortType.NameDesc)
        {
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from apiResource in tenantContext.ApiResources
                where apiResource.Id == apiResourceId
                select apiResource;
            var apiResourceInDb = await query
                .Include(x => x.Scopes)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var entities = from item in apiResourceInDb.Scopes
                select item;
            switch (sortType)
            {
                case ApiResourceScopesSortType.NameDesc:
                    entities = entities.OrderByDescending(t => t.Scope);
                    break;
                case ApiResourceScopesSortType.NameAsc:
                    entities = entities.OrderBy(t => t.Scope);
                    break;
               
            }

            return entities;

        }

        public async Task UpsertClientAsync(string tenantName, ClientExtra entity)
        {
            entity.ClientId = entity.ClientId.ToLower();
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from item in tenantContext.Clients
                where item.ClientId == entity.ClientId
                select item;

            var clientInDb = await query
                .FirstOrDefaultAsync();

            if (clientInDb != null)
            {
                _entityFrameworkMapperAccessor.MapperIgnoreBase.Map(entity, clientInDb);
                await tenantContext.SaveChangesAsync();
            }
            else
            {
                tenantContext.Clients.Add(entity);
            }
            await tenantContext.SaveChangesAsync();
        }


      
        public async Task<ClientExtra> GetClientByClientIdAsync(string tenantName, string clientId)
        {
            clientId = clientId.ToLower();
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from item in tenantContext.Clients
                where item.ClientId == clientId
                select item;
            var clientInDb = await query
                .Include(x => x.ClientSecrets)
                .Include(x => x.AllowedGrantTypes)
                .Include(x => x.AllowedScopes)
                .Include(x => x.Claims)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return clientInDb;
        }
        public async Task<ClientExtra> GetClientByIdAsync(string tenantName, int id)
        {
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from item in tenantContext.Clients
                where item.Id == id
                select item;
            var clientInDb = await query
                .Include(x => x.ClientSecrets)
                .Include(x => x.AllowedScopes)
                .Include(x => x.AllowedGrantTypes)
                .Include(x => x.Claims)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return clientInDb;
        }

        public async Task DeleteClientByNameAsync(string tenantName, string name)
        {
            name = name.ToLower();
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from item in tenantContext.Clients
                where item.ClientId == name
                select item;
            var clientInDb = await query
                .FirstOrDefaultAsync();
            if (clientInDb != null)
            {
                tenantContext.Clients.Remove(clientInDb);
                await tenantContext.SaveChangesAsync();
            }
        }

        public async Task DeleteClientByIdAsync(string tenantName, int id)
        {
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from item in tenantContext.Clients
                where item.Id == id
                select item;
            var clientInDb = await query
                .FirstOrDefaultAsync();
            if (clientInDb != null)
            {
                tenantContext.Clients.Remove(clientInDb);
                await tenantContext.SaveChangesAsync();
            }
        }

        public async Task<PaginatedList<ClientExtra>> PageClientsAsync(string tenantName, int pageNumber, int pageSize, ClientsSortType sortType)
        {
            var tenantContext = GetTenantContext(tenantName);

            var entities = from t in tenantContext.Clients
                select t;
            switch (sortType)
            {
                case ClientsSortType.NameDesc:
                    entities = entities.OrderByDescending(t => t.ClientId);
                    break;
                case ClientsSortType.NameAsc:
                    entities = entities.OrderBy(t => t.ClientId);
                    break;
                case ClientsSortType.EnabledDesc:
                    entities = entities.OrderByDescending(t => t.Enabled);
                    break;
                case ClientsSortType.EnabledAsc:
                    entities = entities.OrderBy(t => t.Enabled);
                    break;
            }

            var paginatedList = await PaginatedList<ClientExtra>
                .CreateAsync(entities.AsNoTracking(), pageNumber, pageSize);
            return paginatedList;
        }

        public async Task UpsertClientSecretAsync(string tenantName, int clientId, ClientSecret entity)
        {
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from item in tenantContext.Clients
                where item.Id == clientId
                select item;

            var client = await query
                .Include(x => x.ClientSecrets)
                .FirstOrDefaultAsync();

            if (client == null)
            {
                throw new System.Exception(
                    $"Client does not exist tenant={tenantName}, clientId={clientId}");
            }
            var secret = client.ClientSecrets.FirstOrDefault(x => x.Id == entity.Id);
            if (secret != null)
            {
                secret.Expiration = entity.Expiration;
            }
            else
            {
                client.ClientSecrets.Add(entity);
            }

            await tenantContext.SaveChangesAsync();
        }

        public async Task<ClientSecret> GetClientSecretByIdAsync(string tenantName, int clientId, int id)
        {
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from item in tenantContext.Clients
                where item.Id == clientId
                select item;

            var client = await query
                .Include(x => x.ClientSecrets)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (client == null)
            {
                throw new System.Exception(
                    $"Client does not exist tenant={tenantName}, clientId={clientId}");
            }

            return client.ClientSecrets.FirstOrDefault(x=>x.Id == id);
        }

        public async Task DeleteClientSecretByIdAsync(string tenantName, int clientId, int id)
        {
            var tenantContext = GetTenantContext(tenantName);

            var query =
                from item in tenantContext.Clients
                where item.Id == clientId
                select item;

            var client = await query
                .Include(x => x.ClientSecrets)
                .FirstOrDefaultAsync();

            if (client == null)
            {
                throw new System.Exception(
                    $"Client does not exist tenant={tenantName}, clientId={clientId}");
            }

            var result = client.ClientSecrets.FirstOrDefault(s => s.Id == id);
            if (result != null)
            {
                client.ClientSecrets.Remove(result);
                await tenantContext.SaveChangesAsync();

            }
        }

        public async Task<IEnumerable<ClientSecret>> GetAllClientSecretsAsync(string tenantName, int id, SecretsSortType sortType)
        {

            var tenantContext = GetTenantContext(tenantName);

            var query =
                from item in tenantContext.Clients
                where item.Id == id
                select item;

          

            var client = await query
                .Include(x => x.ClientSecrets)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (client == null)
            {
                throw new System.Exception(
                    $"Client does not exist tenant={tenantName}, id={id}");
            }

            var entities = from t in client.ClientSecrets
                select t;
            switch (sortType)
            {
                case SecretsSortType.ExpirationDesc:
                    entities = entities.OrderByDescending(t => t.Expiration);
                    break;
                case SecretsSortType.ExpirationAsc:
                    entities = entities.OrderBy(t => t.Expiration);
                    break;
                case SecretsSortType.DescriptionDesc:
                    entities = entities.OrderByDescending(t => t.Description);
                    break;
                case SecretsSortType.DescriptionAsc:
                    entities = entities.OrderBy(t => t.Description);
                    break;
            }

            return entities;
        }

        public async Task UpsertClientAllowedGrantTypesAsync(string tenantName, int clientId, ClientGrantType entity)
        {
            entity.GrantType = entity.GrantType.ToLower();
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from item in tenantContext.Clients
                where item.Id == clientId
                select item;

            var client = await query
                .Include(x => x.AllowedGrantTypes)
                .FirstOrDefaultAsync();

            if (client == null)
            {
                throw new System.Exception(
                    $"Client does not exist tenant={tenantName}, clientId={clientId}");
            }

            var entityInDb = client.AllowedGrantTypes.FirstOrDefault(x => x.GrantType == entity.GrantType);
            if (entityInDb != null && entityInDb.Id != entity.Id)
            {
                // we need to delete the ID as we are attempting to change a grant type to one that already exits.
                entityInDb = client.AllowedGrantTypes.FirstOrDefault(x => x.Id == entity.Id);
                if (entityInDb != null)
                {
                    client.AllowedGrantTypes.Remove(entityInDb);
                    await tenantContext.SaveChangesAsync();
                    return;
                }
            }

            entityInDb = client.AllowedGrantTypes.FirstOrDefault(x => x.Id == entity.Id);
            if (entityInDb != null)
            {
                entityInDb.GrantType = entity.GrantType;
            }
            else
            {
                client.AllowedGrantTypes.Add(entity);
            }
            await tenantContext.SaveChangesAsync();
        }

        public async Task<ClientGrantType> GetClientGrantTypeByIdAsync(string tenantName, int clientId, int id)
        {
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from item in tenantContext.Clients
                where item.Id == clientId
                select item;

            var client = await query
                .Include(x => x.AllowedGrantTypes)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (client == null)
            {
                throw new System.Exception(
                    $"Client does not exist tenant={tenantName}, clientId={clientId}");
            }

            return client.AllowedGrantTypes.FirstOrDefault(x => x.Id == id);

        }

        public async Task DeleteClientGrantTypByIdAsync(string tenantName, int clientId, int id)
        {
            var tenantContext = GetTenantContext(tenantName);
            var query =
                from item in tenantContext.Clients
                where item.Id == clientId
                select item;

            var client = await query
                .Include(x => x.AllowedGrantTypes)
                .FirstOrDefaultAsync();

            if (client == null)
            {
                throw new System.Exception(
                    $"Client does not exist tenant={tenantName}, clientId={clientId}");
            }

            var entityInDb = client.AllowedGrantTypes.FirstOrDefault(x => x.Id == id);
            if (entityInDb != null)
            {
                client.AllowedGrantTypes.Remove(entityInDb);
            }
            await tenantContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<ClientGrantType>> GetAllClientAllowedGrantTypesAsync(string tenantName, int id, GrantTypesSortType sortType)
        {
            var tenantContext = GetTenantContext(tenantName);

            var query =
                from item in tenantContext.Clients
                where item.Id == id
                select item;

            var client = await query
                .Include(x => x.AllowedGrantTypes)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (client == null)
            {
                throw new System.Exception(
                    $"Client does not exist tenant={tenantName}, id={id}");
            }

            var entities = from t in client.AllowedGrantTypes
                select t;
            switch (sortType)
            {
                case GrantTypesSortType.NameDesc:
                    entities = entities.OrderByDescending(t => t.GrantType);
                    break;
                case GrantTypesSortType.NameAsc:
                    entities = entities.OrderBy(t => t.GrantType);
                    break;
            }

            return entities;
        }

       
    }
}