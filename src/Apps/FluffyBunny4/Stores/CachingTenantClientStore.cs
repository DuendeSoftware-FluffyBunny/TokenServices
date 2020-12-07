using FluffyBunny4.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;

namespace FluffyBunny4.Stores
{
    public class CachingTenantClientStore<T> : IClientStore
       where T : IClientStore
    {
        private readonly IdentityServerOptions _options;
        private readonly ICache<Client> _cache;
        private readonly IClientStore _inner;
        private readonly IScopedTenantRequestContext _scopedTenantRequestContext;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingClientStore{T}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="inner">The inner.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        public CachingTenantClientStore(
            IdentityServerOptions options, 
            T inner, 
            IScopedTenantRequestContext scopedTenantRequestContext,
            ICache<Client> cache, 
            ILogger<CachingTenantClientStore<T>> logger)
        {
            _options = options;
            _inner = inner;
            _scopedTenantRequestContext = scopedTenantRequestContext;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Finds a client by id
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <returns>
        /// The client
        /// </returns>
        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            var key = $"{_scopedTenantRequestContext.TenantId}.{clientId}";
            var client = await _cache.GetAsync(key,
                _options.Caching.ClientStoreExpiration,
                () => _inner.FindClientByIdAsync(clientId),
                _logger);

            return client;
        }
    }
}
