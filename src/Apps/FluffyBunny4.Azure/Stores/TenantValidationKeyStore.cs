using FluffyBunny4.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using FluffyBunny4.DotNetCore.Services;
using FluffyBunny4.Models;

namespace FluffyBunny4.Stores
{
    public class TenantValidationKeyStore : IValidationKeysStore
    {
        private IScopedContext<TenantRequestContext> _scopedTenantRequestContext;
        private IKeyVaultTenantResolver _tenantResolver;
        private ILogger _logger;

        public TenantValidationKeyStore(
            IScopedContext<TenantRequestContext> scopedTenantRequestContext,
            IKeyVaultTenantResolver tenantResolver,
            ILogger<TenantValidationKeyStore> logger)
        {
            _scopedTenantRequestContext = scopedTenantRequestContext;
            _tenantResolver = tenantResolver;
            _logger = logger;

        }
        public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            var keyVaultECDsaKeyStore = await _tenantResolver.GetKeyVaultECDsaKeyStoreAsync(_scopedTenantRequestContext.Context.TenantName);
            var cache = await keyVaultECDsaKeyStore.FetchCacheAsync();
            return cache.SecurityKeyInfos;
        }
    }
}
