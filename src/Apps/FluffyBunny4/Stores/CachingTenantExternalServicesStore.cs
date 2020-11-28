using FluffyBunny4.Models;
using FluffyBunny4.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;

namespace FluffyBunny4.Stores
{
    public class CachingTenantExternalServicesStore<T> : IExternalServicesStore
          where T : IExternalServicesStore
    {
        private readonly IdentityServerOptions _options;
        private readonly ICache<ExternalService> _cacheExternalService;
        private readonly ICache<List<ExternalService>> _cacheExternalServices;
        private readonly IExternalServicesStore _inner;
        private readonly ITenantRequestContext _tenantRequestContext;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingTenantExternalServicesStore{T}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="inner">The inner.</param>
        /// <param name="cacheExternalService">The cache.</param>
        /// <param name="logger">The logger.</param>
        public CachingTenantExternalServicesStore(
            IdentityServerOptions options,
            T inner,
            ITenantRequestContext tenantRequestContext,
            ICache<ExternalService> cacheExternalService,
            ICache<List<ExternalService>> cacheExternalServices,
            ILogger<CachingTenantExternalServicesStore<T>> logger)
        {
            _options = options;
            _inner = inner;
            _tenantRequestContext = tenantRequestContext;
            _cacheExternalService = cacheExternalService;
            _cacheExternalServices = cacheExternalServices;
            _logger = logger;
        }

        public async Task<ExternalService> GetExternalServiceByNameAsync(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName)) return null;

            var key = $"{_tenantRequestContext.TenantId}.{serviceName}";
            var item = await _cacheExternalService.GetAsync(key,
                _options.Caching.ClientStoreExpiration,
                () => _inner.GetExternalServiceByNameAsync(serviceName),
                _logger);

            return item;
        }

        public async Task<List<ExternalService>> GetExternalServicesAsync()
        {
            var key = $"{_tenantRequestContext.TenantId}.GetExternalServicesAsync";
            var item = await _cacheExternalServices.GetAsync(key,
                _options.Caching.ClientStoreExpiration,
                () => _inner.GetExternalServicesAsync(),
                _logger);

            return item;
        }
    }
}
