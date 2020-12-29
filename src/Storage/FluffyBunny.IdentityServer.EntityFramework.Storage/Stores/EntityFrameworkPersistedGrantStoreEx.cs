using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using FluffyBunny.EntityFramework.Context;
using FluffyBunny.EntityFramework.Entities;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using FluffyBunny4;
using FluffyBunny4.DotNetCore.Services;
using FluffyBunny4.Services;
using FluffyBunny4.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Stores
{
    class EntityFrameworkPersistedGrantStoreEx : IPersistedGrantStoreEx
    {
        private IScopedTenantRequestContext _scopedTenantRequestContext;
        private IScopedStorage _scopedStorage;
        private IAdminServices _adminServices;
        private IEntityFrameworkMapperAccessor _entityFrameworkMapperAccessor;
        private ITenantAwareConfigurationDbContextAccessor _tenantAwareConfigurationDbContextAccessor;
        private ILogger<EntityFrameworkPersistedGrantStoreEx> Logger;

        public EntityFrameworkPersistedGrantStoreEx(
            IScopedTenantRequestContext scopedTenantRequestContext,
            IScopedStorage scopedStorage,
            IAdminServices adminServices,
            IEntityFrameworkMapperAccessor entityFrameworkMapperAccessor,
            ITenantAwareConfigurationDbContextAccessor tenantAwareConfigurationDbContextAccessor,
            ILogger<EntityFrameworkPersistedGrantStoreEx> logger)
        {
            _scopedTenantRequestContext = scopedTenantRequestContext;
            _scopedStorage = scopedStorage;
            _adminServices = adminServices;
            _entityFrameworkMapperAccessor = entityFrameworkMapperAccessor;
            _tenantAwareConfigurationDbContextAccessor = tenantAwareConfigurationDbContextAccessor;
            Logger = logger;
        }
        ITenantAwareConfigurationDbContext GetTenantContext()
        {
            var name = _scopedTenantRequestContext.TenantId;
            name = name.ToLower();
            return _tenantAwareConfigurationDbContextAccessor.GetTenantAwareConfigurationDbContext(name);
        }

        public async Task CopyAsync(string sourceKey, string destinationKey)
        {
            var sourceGrant = await GetAsync(sourceKey);
            if (sourceGrant == null) return;

            var destinationGrant = await GetAsync(destinationKey);
            if (destinationGrant == null) return;

            _entityFrameworkMapperAccessor.MapperOneToOne.Map(sourceGrant, destinationGrant);
            destinationGrant.Key = destinationKey;
            await StoreAsync(destinationGrant);
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            var context = GetTenantContext();

            filter.Validate();

            var persistedGrants = await Filter(context.PersistedGrants.AsQueryable(), filter).ToArrayAsync();
            persistedGrants = Filter(persistedGrants.AsQueryable(), filter).ToArray();

            var model = persistedGrants.Select(x => x.ToModel());

            Logger.LogDebug("{persistedGrantCount} persisted grants found for {@filter}", persistedGrants.Length, filter);

            return model; ;
        }

        public async Task<PersistedGrant> GetAsync(string key)
        {
            var context = GetTenantContext();
            var persistedGrant = (await context.PersistedGrants.AsNoTracking().Where(x => x.Key == key).ToArrayAsync())
                .SingleOrDefault(x => x.Key == key);

            if (persistedGrant == null) return null;

            var model = _entityFrameworkMapperAccessor.MapperOneToOne
                .Map<FluffyBunny4.Models.PersistedGrantExtra>(persistedGrant);

            Logger.LogDebug("{persistedGrantKey} found in database: {persistedGrantKeyFound}", key, model != null);

            return model;

        }

        public async Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            var context = GetTenantContext();

            filter.Validate();

            var persistedGrants = await Filter(context.PersistedGrants.AsQueryable(), filter).ToArrayAsync();
            persistedGrants = Filter(persistedGrants.AsQueryable(), filter).ToArray();

            Logger.LogDebug("removing {persistedGrantCount} persisted grants from database for {@filter}", persistedGrants.Length, filter);

            context.PersistedGrants.RemoveRange(persistedGrants);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Logger.LogInformation("removing {persistedGrantCount} persisted grants from database for subject {@filter}: {error}", persistedGrants.Length, filter, ex.Message);
            }
        }

        public async Task RemoveAsync(string key)
        {
            var context = GetTenantContext();

            var persistedGrant = (await context.PersistedGrants.Where(x => x.Key == key).ToArrayAsync())
                .SingleOrDefault(x => x.Key == key);
            if (persistedGrant != null)
            {
                Logger.LogDebug("removing {persistedGrantKey} persisted grant from database", key);

                context.PersistedGrants.Remove(persistedGrant);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Logger.LogInformation("exception removing {persistedGrantKey} persisted grant from database: {error}", key, ex.Message);
                }
            }
            else
            {
                Logger.LogDebug("no {persistedGrantKey} persisted grant found in database", key);
            }
        }

        private async Task InnerStoreAsync(PersistedGrant grant)
        {
            var context = GetTenantContext();

            var token = grant;
            var extra = grant as FluffyBunny4.Models.PersistedGrantExtra;
        

            var existing = (await context.PersistedGrants.Where(x => x.Key == token.Key).ToArrayAsync())
                .SingleOrDefault(x => x.Key == token.Key);
            if (existing == null)
            {
                Logger.LogDebug("{persistedGrantKey} not found in database", token.Key);

                PersistedGrantExtra persistedGrant = null;
                if (extra != null)
                {
                    persistedGrant = _entityFrameworkMapperAccessor.MapperOneToOne
                        .Map<PersistedGrantExtra>(extra);
                }
                else
                {
                    persistedGrant = _entityFrameworkMapperAccessor.MapperOneToOne
                        .Map<PersistedGrantExtra>(token);
                }
                context.PersistedGrants.Add(persistedGrant);
            }
            else
            {
                Logger.LogDebug("{persistedGrantKey} found in database", token.Key);
                if (extra != null)
                {
                    _entityFrameworkMapperAccessor.MapperOneToOne.Map(extra, existing);
                }
                else
                {
                    _entityFrameworkMapperAccessor.MapperOneToOne.Map(token, existing);
                }
            }

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Logger.LogWarning("exception updating {persistedGrantKey} persisted grant in database: {error}", token.Key, ex.Message);
            }
        }

        public async Task StoreAsync(PersistedGrant grant)
        {
            var context = GetTenantContext();

            var token = grant as FluffyBunny4.Models.PersistedGrantExtra;


            object obj;
            if (_scopedStorage.TryGetValue(Constants.ScopedRequestType.ExtensionGrantValidationContext, out obj))
            {
                var extensionGrantValidationContext = obj as ExtensionGrantValidationContext;
                if (extensionGrantValidationContext.Request.GrantType == FluffyBunny4.Constants.GrantType.TokenExchange)
                {
                    if (grant.Type != IdentityServerConstants.PersistedGrantTypes.RefreshToken)
                    {
                        var offlineAccess =
                            extensionGrantValidationContext.Request.RequestedScopes.FirstOrDefault(scope =>
                                scope == IdentityServerConstants.StandardScopes.OfflineAccess);
                        if (offlineAccess != null)
                        {
                            // refresh_token coming. so save this access_token for later storage
                            _scopedStorage.AddOrUpdate(Constants.ScopedRequestType.AccessTokenPersistedGrant,
                                grant);
                            return;
                        }
                        // store access_token for later
                    }
                    else
                    {
                        _scopedStorage.TryGetValue(Constants.ScopedRequestType.AccessTokenPersistedGrant, out obj);


                        var grantStored = obj as Duende.IdentityServer.Models.PersistedGrant;
                        var extra = _entityFrameworkMapperAccessor.MapperOneToOne
                            .Map<FluffyBunny4.Models.PersistedGrantExtra>(grantStored);

                        extra.RefreshTokenKey = grant.Key;
                        await InnerStoreAsync(extra);
                        await InnerStoreAsync(grant);

                        _scopedStorage.TryRemove(Constants.ScopedRequestType.AccessTokenPersistedGrant, out obj);
                        _scopedStorage.TryRemove(Constants.ScopedRequestType.ExtensionGrantValidationContext, out obj);
                        return;
                    }
                }
            }

            await InnerStoreAsync(grant);
           
        }
        private IQueryable<PersistedGrantExtra> Filter(IQueryable<PersistedGrantExtra> query, PersistedGrantFilter filter)
        {
            if (!String.IsNullOrWhiteSpace(filter.ClientId))
            {
                query = query.Where(x => x.ClientId == filter.ClientId);
            }
            if (!String.IsNullOrWhiteSpace(filter.SessionId))
            {
                query = query.Where(x => x.SessionId == filter.SessionId);
            }
            if (!String.IsNullOrWhiteSpace(filter.SubjectId))
            {
                query = query.Where(x => x.SubjectId == filter.SubjectId);
            }
            if (!String.IsNullOrWhiteSpace(filter.Type))
            {
                query = query.Where(x => x.Type == filter.Type);
            }

            return query;
        }
    }
}
