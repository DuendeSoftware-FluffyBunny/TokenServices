using Microsoft.Extensions.DependencyInjection;
using OIDC.ReferenceWebClient.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OIDCPipeline.Core.Extensions
{
    public static class GoogleDiscoveryCacheExtensions
    {
        public static IServiceCollection AddGoogleDiscoveryCache(this IServiceCollection services)
        {
            services.AddSingleton<IGoogleDiscoveryCache>(r =>
            {
                var factory = r.GetRequiredService<IHttpClientFactory>();
                return new GoogleDiscoveryCache("https://accounts.google.com", () => factory.CreateClient());
            });
            return services;
        }
    }
}
