using FluffyBunny4.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;

namespace FluffyBunny4.Stores
{
    public class CachingTenantStore<T> : ITenantStore
          where T : ITenantStore
    {
        private readonly IdentityServerOptions _options;
        private readonly ICache<TenantHandle> _cache;
        private readonly ITenantStore _inner;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingTenantStore{T}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="inner">The inner.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        public CachingTenantStore(
            IdentityServerOptions options,
            T inner,
            ICache<TenantHandle> cache,
            ILogger<CachingTenantStore<T>> logger)
        {
            _options = options;
            _inner = inner;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Finds a tenant by id
        /// </summary>
        /// <param name="tenantId">The tenant id</param>
        /// <returns>
        /// The client
        /// </returns>


        public async Task<TenantHandle> FindTenantByIdAsync(string tenantId)
        {
            if (string.IsNullOrEmpty(tenantId)) return null;
 
            var tenant = await _cache.GetAsync(tenantId,
                _options.Caching.ClientStoreExpiration,
                () => _inner.FindTenantByIdAsync(tenantId),
                _logger);

            return tenant;
 
        }

    }
}
