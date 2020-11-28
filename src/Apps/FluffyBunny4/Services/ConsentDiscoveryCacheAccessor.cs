using FluffyBunny4.Cache;
using FluffyBunny4.Models;
using FluffyBunny4.Stores;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FluffyBunny4.Services
{
    public class ConsentDiscoveryCacheAccessor : IConsentDiscoveryCacheAccessor
    {
        private IHttpClientFactory _httpClientFactory;
        private IExternalServicesStore _externalServicesStore;
        static ConcurrentDictionary<string, IConsentDiscoveryCache> _map = new ConcurrentDictionary<string, IConsentDiscoveryCache>();
        public ConsentDiscoveryCacheAccessor(
            IHttpClientFactory httpClientFactory,
            IExternalServicesStore externalServicesStore)
        {
            _httpClientFactory = httpClientFactory;
            _externalServicesStore = externalServicesStore;
        }

        public async Task<IConsentDiscoveryCache> GetConsentDiscoveryCacheAsync(string serviceName)
        {
            IConsentDiscoveryCache value = null;
            if (!_map.TryGetValue(serviceName, out value))
            {
                var externalService = await _externalServicesStore.GetExternalServiceByNameAsync(serviceName);

                value = new ConsentDiscoveryCache(externalService.Authority, () => _httpClientFactory.CreateClient(FluffyBunny4.Constants.ExternalServiceClient.HttpClientName));
                _map.TryAdd(externalService.Name, value);
            }
            return value;
        }
    }  
}