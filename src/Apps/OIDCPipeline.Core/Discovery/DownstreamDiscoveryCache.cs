using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OIDC.ReferenceWebClient.Discovery
{
    internal class DownstreamDiscoveryCache : DiscoveryCache, IDownstreamDiscoveryCache
    {

        public DownstreamDiscoveryCache(string authority, DiscoveryPolicy policy = null) : base(authority, policy)
        {
        }

        public DownstreamDiscoveryCache(string authority, Func<HttpClient> httpClientFunc, DiscoveryPolicy policy = null) : base(authority, httpClientFunc, policy)
        {
        }
    }
}
