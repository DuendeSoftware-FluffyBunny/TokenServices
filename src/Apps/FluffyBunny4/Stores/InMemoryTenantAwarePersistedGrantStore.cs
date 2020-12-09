using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer;
using Duende.IdentityServer.Validation;
using FluffyBunny4.DotNetCore.Services;
using FluffyBunny4.Models;
using FluffyBunny4.Services;

namespace FluffyBunny4.Stores
{
    class InMemoryTenantAwarePersistedGrantStore : IPersistedGrantStoreEx
    {
        static ConcurrentDictionary<string, IPersistedGrantStore> _tenantStores = new ConcurrentDictionary<string, IPersistedGrantStore>();
        private IScopedStorage _scopedStorage;
        private IScopedTenantRequestContext _scopedTenantRequestContext;
        private ICoreMapperAccessor _coreMapperAccessor;
        private IPersistedGrantStore _innerPersistedGrantStore;
        public InMemoryTenantAwarePersistedGrantStore(
            IScopedStorage scopedStorage, 
            IScopedTenantRequestContext scopedTenantRequestContext,
            ICoreMapperAccessor coreMapperAccessor)
        {
            _scopedStorage = scopedStorage;
            _scopedTenantRequestContext = scopedTenantRequestContext;
            _coreMapperAccessor = coreMapperAccessor;
            if (!string.IsNullOrWhiteSpace(_scopedTenantRequestContext.TenantId))
            {
                if (!_tenantStores.ContainsKey(_scopedTenantRequestContext.TenantId))
                {
                    _tenantStores.TryAdd(_scopedTenantRequestContext.TenantId, new InMemoryPersistedGrantStore());
                }

                _tenantStores.TryGetValue(_scopedTenantRequestContext.TenantId, out _innerPersistedGrantStore);

            }
        }

        public Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            return _innerPersistedGrantStore.GetAllAsync(filter);
        }

        public async Task CopyAsync(string sourceKey, string destinationKey)
        {
            var source = await _innerPersistedGrantStore.GetAsync(sourceKey);

            if (source != null)
            {
                source.Key = destinationKey;
                await StoreAsync(source);
            }
        }

        public Task<PersistedGrant> GetAsync(string key)
        {
            return _innerPersistedGrantStore.GetAsync(key);
        }

        public Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            return _innerPersistedGrantStore.RemoveAllAsync(filter);
        }

        public Task RemoveAsync(string key)
        {
            return _innerPersistedGrantStore.RemoveAsync(key);
        }

        public async Task StoreAsync(PersistedGrant grant)
        {
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
                       if(offlineAccess != null)
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
                        var grantStored = obj as PersistedGrant;
                        var extra = _coreMapperAccessor.Mapper.Map<PersistedGrantExtra>(grantStored);
                        extra.RefreshTokenKey = grant.Key;
                        await _innerPersistedGrantStore.StoreAsync(extra);

                        extra = _coreMapperAccessor.Mapper.Map<PersistedGrantExtra>(grant);
                        extra.AccessTokenKey = grantStored.Key;
                        await _innerPersistedGrantStore.StoreAsync(extra);

                        return;
                    }
                }

            }
            
            await _innerPersistedGrantStore.StoreAsync(grant);
            
        }
    }
}
