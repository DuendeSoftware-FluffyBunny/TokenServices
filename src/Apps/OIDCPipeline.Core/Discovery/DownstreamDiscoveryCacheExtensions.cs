using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using OIDCPipeline.Core.Configuration;
using OIDCPipeline.Core.Services;
using System;
using System.Net.Http;

namespace OIDC.ReferenceWebClient.Discovery
{
    public static class DownstreamDiscoveryCacheExtensions
    {
        public static IServiceCollection AddDownstreamDiscoveryCache(this IServiceCollection services)
        {

            services.AddSingleton<IDownstreamDiscoveryCache>(r =>
            {
                var factory = r.GetRequiredService<IHttpClientFactory>();
                var options = r.GetRequiredService<OIDCPipelineOptions>();
                var openIdConnectSchemeRecords = r.GetRequiredService<IOpenIdConnectSchemeRecords>();
                var record = openIdConnectSchemeRecords.GetOpenIdConnectSchemeRecordBySchemeAsync(options.Scheme).GetAwaiter().GetResult();
                return new DownstreamDiscoveryCache(record.Authority,
                    () => factory.CreateClient("downstream"),
                    new DiscoveryPolicy
                    {
                        ValidateEndpoints = false
                    });
            });
            return services;
        }
    }
}
