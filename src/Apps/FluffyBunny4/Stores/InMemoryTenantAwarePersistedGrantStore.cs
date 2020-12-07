using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluffyBunny4.Services;

namespace FluffyBunny4.Stores
{
    class InMemoryTenantAwarePersistedGrantStore : IPersistedGrantStore
    {
        static ConcurrentDictionary<string, IPersistedGrantStore> _tenantStores = new ConcurrentDictionary<string, IPersistedGrantStore>();
        
        private ITenantRequestContext _tenantRequestContext;
        private IPersistedGrantStore _innerPersistedGrantStore;
        public InMemoryTenantAwarePersistedGrantStore(ITenantRequestContext tenantRequestContext)
        {
            _tenantRequestContext = tenantRequestContext;
            if (!string.IsNullOrWhiteSpace(_tenantRequestContext.TenantId))
            {
                if (!_tenantStores.ContainsKey(_tenantRequestContext.TenantId))
                {
                    _tenantStores.TryAdd(_tenantRequestContext.TenantId, new InMemoryPersistedGrantStore());
                }

                _tenantStores.TryGetValue(_tenantRequestContext.TenantId, out _innerPersistedGrantStore);

            }
        }

        public Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            return _innerPersistedGrantStore.GetAllAsync(filter);
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

        public Task StoreAsync(PersistedGrant grant)
        {
            return _innerPersistedGrantStore.StoreAsync(grant);
        }
    }
}
