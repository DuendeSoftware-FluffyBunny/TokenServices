using FluffyBunny4.DotNetCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;

namespace FluffyBunny4.Services
{
    public class CachingTenantResolver<T> : IKeyVaultTenantResolver
          where T : IKeyVaultTenantResolver
    {
        private TimedLock _lock;
        private readonly ICache<IJwksDiscovery> _cacheIJwksDiscovery;
        private readonly ICache<IKeyVaultECDsaKeyStore> _cacheIKeyVaultECDsaKeyStore;
        private readonly ICache<ISignatureProvider> _cacheISignatureProvider;

        private readonly IKeyVaultTenantResolver _inner;
        private readonly ILogger _logger;
        static TimeSpan CachingTenantResolverExpiration { get; set; } = TimeSpan.FromHours(12);
        public CachingTenantResolver(
            T inner,
            ICache<IJwksDiscovery> cacheIJwksDiscovery,
            ICache<IKeyVaultECDsaKeyStore> cacheIKeyVaultECDsaKeyStore,
            ICache<ISignatureProvider> cacheISignatureProvider,
            ILogger<CachingTenantResolver<T>> logger)
        {
            _inner = inner;
            _cacheIJwksDiscovery = cacheIJwksDiscovery;
            _cacheIKeyVaultECDsaKeyStore = cacheIKeyVaultECDsaKeyStore;
            _cacheISignatureProvider = cacheISignatureProvider;
            _logger = logger;
        }


        public async Task<IJwksDiscovery> GetJwksDisoveryAsync(string tenantId)
        {
            var iJwksDiscovery = await _cacheIJwksDiscovery.GetAsync(tenantId,
                CachingTenantResolverExpiration,
                () => _inner.GetJwksDisoveryAsync(tenantId),
                _logger);
            return iJwksDiscovery;
        }

        public async Task<IKeyVaultECDsaKeyStore> GetKeyVaultECDsaKeyStoreAsync(string tenantId)
        {
            var iKeyVaultECDsaKeyStore = await _cacheIKeyVaultECDsaKeyStore.GetAsync(tenantId,
              CachingTenantResolverExpiration,
              () => _inner.GetKeyVaultECDsaKeyStoreAsync(tenantId),
              _logger);
            return iKeyVaultECDsaKeyStore;
        }

        public async Task<ISignatureProvider> GetSignatureProviderAsync(string tenantId)
        {
            var iSignatureProvider = await _cacheISignatureProvider.GetAsync(tenantId,
              CachingTenantResolverExpiration,
              () => _inner.GetSignatureProviderAsync(tenantId),
              _logger);
            return iSignatureProvider;
        }
    }
}
