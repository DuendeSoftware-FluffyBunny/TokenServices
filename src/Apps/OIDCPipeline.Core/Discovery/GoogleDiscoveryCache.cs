using IdentityModel.Client;
using System;
using System.Net.Http;

namespace OIDC.ReferenceWebClient.Discovery
{
    public class GoogleDiscoveryCache : DiscoveryCache, IGoogleDiscoveryCache
    {
        public GoogleDiscoveryCache(string authority, DiscoveryPolicy policy = null) : base(authority, policy)
        {
        }
        public GoogleDiscoveryCache(string authority, Func<HttpClient> httpClientFunc, DiscoveryPolicy policy = null) : base(authority, httpClientFunc, policy)
        {
        }
    }
}
