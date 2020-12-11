using IdentityModel.Client;
using System;
using System.Net.Http;

namespace OIDCConsentOrchestrator.Services
{
    internal class TokenServiceDiscoveryCache : DiscoveryCache, ITokenServiceDiscoveryCache
    {

        public TokenServiceDiscoveryCache(string authority, DiscoveryPolicy policy = null) : base(authority, policy)
        {
        }

        public TokenServiceDiscoveryCache(string authority, Func<HttpClient> httpClientFunc, DiscoveryPolicy policy = null) : base(authority, httpClientFunc, policy)
        {
        }
    }
}
