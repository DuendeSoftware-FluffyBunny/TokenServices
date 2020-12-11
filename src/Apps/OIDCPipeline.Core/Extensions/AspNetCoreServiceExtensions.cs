using System;
using System.Collections.Generic;
using System.Text;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OIDC.ReferenceWebClient.Discovery;
using OIDCPipeline.Core.Configuration;
using OIDCPipeline.Core.Endpoints;
using OIDCPipeline.Core.Hosting;
using OIDCPipeline.Core.Validation;
using OIDCPipeline.Core.Validation.Default;
using static OIDCPipeline.Core.Constants;

namespace OIDCPipeline.Core.Extensions
{
    public static class AspNetCoreServiceExtensions
    {
        /// <summary>
        /// Adds the endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services">The services.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        internal static IServiceCollection AddEndpoint<T>(this IServiceCollection services, string name, PathString path)
            where T : class, IEndpointHandler
        {
            services.AddTransient<T>();
            services.AddSingleton(new Hosting.Endpoint(name, path, typeof(T)));

            return services;
        }
        /// <summary>
        /// Adds the required platform services.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        internal static IServiceCollection AddRequiredPlatformServices(this IServiceCollection services)
        {

          
            services.AddSingleton(
                resolver => resolver.GetRequiredService<IOptions<OIDCPipelineOptions>>().Value);

            return services;
        }
        private static void AddOIDCPipelineOptions(
            this IServiceCollection services, 
            Action<OIDCPipelineOptions> setupAction)
        {
            services.AddOptions();
            services.Configure(setupAction);
        }
        public static void AddOIDCPipeline(
            this IServiceCollection services, 
            Action<OIDCPipelineOptions> setupAction)
        {
            services.TryAddTransient(typeof(ICache<>), typeof(DefaultCache<>));
            services.AddTransient<IOIDCPipeLineKey, OIDCPipeLineKey>();
            services.AddOIDCPipelineOptions(setupAction);  // do first
            services.AddRequiredPlatformServices();
            services.AddTransient<IOIDCResponseGenerator, OIDCResponseGenerator>();
            services.TryAddTransient<IAuthorizeRequestValidator, DefaultAuthorizeRequestValidator>();
            services.TryAddTransient<ITokenRequestValidator, DefaultTokenRequestValidator>();
            services.AddDownstreamDiscoveryCache();


            services.AddEndpoint<TokenEndpoint>(EndpointNames.Token, ProtocolRoutePaths.Token.EnsureLeadingSlash());
            services.AddEndpoint<DiscoveryEndpoint>(EndpointNames.Discovery, ProtocolRoutePaths.DiscoveryConfiguration.EnsureLeadingSlash());
            services.AddEndpoint<AuthorizeEndpoint>(EndpointNames.Authorize, ProtocolRoutePaths.Authorize.EnsureLeadingSlash());
            services.AddTransient<IEndpointRouter, EndpointRouter>();

        }
        public static void AddMemoryCacheOIDCPipelineStore(this IServiceCollection services, Action<MemoryCacheOIDCPipelineStoreOptions> setupAction)
        {
            services.Configure(setupAction);
            services.AddTransient<IOIDCPipelineStore, MemoryCacheOIDCPipelineStore>();
        }
        public static void AddDistributedCacheOIDCPipelineStore(this IServiceCollection services, Action<MemoryCacheOIDCPipelineStoreOptions> setupAction)
        {
            services.Configure(setupAction);
            services.AddTransient<IOIDCPipelineStore, DistributedCacheOIDCPipelineStore>();
        }

        public static IApplicationBuilder UseOIDCPipeline(this IApplicationBuilder app)
        {
            app.UseMiddleware<OIDCPipelineMiddleware>();
            return app;
        }
    }
}
