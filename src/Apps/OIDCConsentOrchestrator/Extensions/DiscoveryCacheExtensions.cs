using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OIDCConsentOrchestrator.Models;
using OIDCConsentOrchestrator.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.Extensions
{
    public static class DiscoveryCacheExtensions
    {
        public static IServiceCollection AddTokenServiceDiscoveryCache(this IServiceCollection services)
        {

            services.AddSingleton<ITokenServiceDiscoveryCache>(r =>
            {
                var factory = r.GetRequiredService<IHttpClientFactory>();
                var options = r.GetRequiredService<IOptions<FluffyBunny4TokenServiceConfiguration>>();
 
                return new TokenServiceDiscoveryCache(options.Value.Authority,
                    () => factory.CreateClient("token-service"),
                    new DiscoveryPolicy
                    {
                        ValidateEndpoints = false
                    });
            });
            return services;
        }
    }
}
