using IdentityModel.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using FluffyBunny4.Configuration;
using Microsoft.Extensions.Options;

namespace FluffyBunny4.Services.Default
{
    public class DiscoveryCacheAccessor : IDiscoveryCacheAccessor
    {
        private TokenExchangeOptions _tokenExchangeOptions;
        private ConcurrentDictionary<string, IDiscoveryCache> _authorityMap;
        private DateTime Expiration { get; set; }
        public DiscoveryCacheAccessor(IOptions<TokenExchangeOptions> tokenExchangeOptions)
        {
            _authorityMap = new ConcurrentDictionary<string, IDiscoveryCache>();
            _tokenExchangeOptions = tokenExchangeOptions.Value;
            foreach (var authority in _tokenExchangeOptions.Authorities)
            {
                var dc = new DiscoveryCache(authority.Value,new DiscoveryPolicy()
                {
                    ValidateEndpoints = false
                    
                });
                _authorityMap[authority.Key] = dc;
            }
        }

      
        public IDiscoveryCache GetCache(string name)
        {
            if (_authorityMap.TryGetValue(name, out var result))
            {
                return result;
            }
            return null;
        }
    }
}
