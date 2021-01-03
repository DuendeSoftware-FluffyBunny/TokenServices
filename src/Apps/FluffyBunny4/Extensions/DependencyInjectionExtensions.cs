using AutoMapper;
using FluffyBunny4.Cache;
using FluffyBunny4.Configuration;
using FluffyBunny4.DotNetCore.Extensions;
using FluffyBunny4.DotNetCore.Services;
using FluffyBunny4.Models;
using FluffyBunny4.Services;
using FluffyBunny4.Services.Default;
using FluffyBunny4.Stores;
 
using IdentityServer4.Validation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.ResponseHandling;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using FluffyBunny4.AutoMapper;
using FluffyBunny4.Validation;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    { 
        public static IServiceCollection AddPasswordGenerator(this IServiceCollection services)
        {
            services.TryAddSingleton<IPasswordGenerator, PasswordGenerator>();
            return services;
        }
        /// <summary>
        /// Adds the in memory API scopes.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="apiScopes">The API scopes.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddTenantAwareInMemoryApiScopes(this IIdentityServerBuilder builder,
            string jsonApiScopesFile,
            string jsonApiResourcesFile)
        {
            builder.Services.AddInMemoryInMemoryResourcesTenantStore(jsonApiScopesFile, jsonApiResourcesFile);
            builder.AddResourceStore<TenantAwareInMemoryResourcesStore>();

            return builder;
        }

        public static IIdentityServerBuilder SwapOutTokenRevocationRequestValidator(
            this IIdentityServerBuilder builder)
        {
            builder.Services.RemoveAll<ITokenRevocationRequestValidator>();
            builder.Services.TryAddTransient<ITokenRevocationRequestValidator, FluffyBunny4.Validation.TokenRevocationRequestValidator>();
            return builder;
        }


        public static IIdentityServerBuilder SwapOutTokenRevocationResponseGenerator(
            this IIdentityServerBuilder builder)
        {
            builder.Services.RemoveAll<ITokenRevocationResponseGenerator>();
            builder.Services.AddTransient<Duende.IdentityServer.ResponseHandling.TokenRevocationResponseGenerator>();
            builder.Services.TryAddTransient<ITokenRevocationResponseGenerator, FluffyBunny4.ResponseHandling.TokenRevocationResponseGenerator>();
            builder.Services.AddBackgroundServices<FluffyBunny4.ResponseHandling.TokenRevocationResponseGenerator.Delete>();
            return builder;
        } 

        public static IServiceCollection AddInMemoryInMemoryResourcesTenantStore(
            this IServiceCollection services,
            string jsonApiScopesFile,
            string jsonApiResourcesFile)
        {
            services.AddSingleton<IEnumerable<TenantApiScopeHandle>>(sp =>
            {
                var serializer = sp.GetRequiredService<ISerializer>();
                var json = File.ReadAllText(jsonApiScopesFile);
                var items = serializer.Deserialize<List<TenantApiScopeHandle>>(json);
                return items;
            });
            services.AddSingleton<IEnumerable<TenantApiResourceHandle>>(sp =>
            {
                var serializer = sp.GetRequiredService<ISerializer>();
                var json = File.ReadAllText(jsonApiResourcesFile);
                var items = serializer.Deserialize<List<TenantApiResourceHandle>>(json);
                return items;
            });
            return services;
        }

        public static IServiceCollection AddInMemoryTenantStore(
            this IServiceCollection services,
            string fileJson)
        {
            services.AddSingleton<IEnumerable<TenantHandle>>(sp =>
            {
                var serializer = sp.GetRequiredService<ISerializer>();
                var json = File.ReadAllText(fileJson);
                var tenants = serializer.Deserialize<List<TenantHandle>>(json);
                return tenants;
            });

            services.AddTenantStoreCache<InMemoryTenantStore>();
 
            return services;
        }
        public static IServiceCollection AddTenantStoreCache<T>(this IServiceCollection services)
            where T : ITenantStore
        {
            services.TryAddTransient(typeof(T));
            services.AddTransient<ITenantStore, CachingTenantStore<T>>();
            return services;
        }
        public static IServiceCollection AddExternalServicesStoreCache<T>(this IServiceCollection services)
           where T : IExternalServicesStore
        {
            services.TryAddTransient(typeof(T));
            services.AddTransient<IExternalServicesStore, CachingTenantExternalServicesStore<T>>();
            return services;
        }
        

 
        public static IServiceCollection AddClientStore<T>(this IServiceCollection services)
           where T : class, IClientStore
        {
            services.TryAddTransient(typeof(T));
            services.AddTransient<IClientStore, ValidatingClientStore<T>>();

            return services;
        }
 
        public static IServiceCollection AddTenantClientStoreCache<T>(this IServiceCollection services)
           where T : IClientStore
        {
            services.TryAddTransient(typeof(T));
            services.AddTransient<ValidatingClientStore<T>>();
            services.AddTransient<IClientStore, CachingTenantClientStore<ValidatingClientStore<T>>>();
           
            return services;
        }
        public static IServiceCollection AddFluffyBunny4AutoMapper(this IServiceCollection services)
        {
            
            var mapper = MapperConfigurationBuilder.BuidOneToOneMapper;
            services.AddSingleton<IMapper>(mapper);
            var coreMapperAccessor = new CoreMapperAccessor(mapper);
            services.AddSingleton<ICoreMapperAccessor>(coreMapperAccessor);
            return services;
        }

        public static IServiceCollection AddInMemoryCaching(this IServiceCollection services)
        {
            services.TryAddSingleton<IMemoryCache, MemoryCache>();
            return services;
        }

        public static IServiceCollection AddGraceRefreshTokenService(
               this IServiceCollection services)
        {
            services.TryAddTransient<IRefreshTokenService, GraceRefreshTokenService>();

            return services;
        }
        public static IIdentityServerBuilder SwapOutRefreshTokenStore(this IIdentityServerBuilder builder) 
        {
            builder.Services.RemoveAll<IRefreshTokenStore>();
            builder.Services.TryAddTransient<IRefreshTokenStore, MyDefaultRefreshTokenStore>();
            builder.Services.AddBackgroundServices<MyDefaultRefreshTokenStore.Delete>();
            builder.Services.AddBackgroundServices<MyDefaultRefreshTokenStore.Write>();
            return builder;
        }
        public static IIdentityServerBuilder SwapOutReferenceTokenStore(this IIdentityServerBuilder builder)
        {
            builder.Services.RemoveAll<IReferenceTokenStore>();
            builder.Services.TryAddTransient<IReferenceTokenStore, MyDefaultReferenceTokenStore>();
            return builder;
        }
        public static IServiceCollection AddConsentDiscoveryCacheAccessor(this IServiceCollection services)
        {
            services.AddScoped<IConsentDiscoveryCacheAccessor, ConsentDiscoveryCacheAccessor>();
           
            return services;
        }
        public static IServiceCollection AddDiscoveryCacheAccessor(this IServiceCollection services)
        {
            services.AddSingleton<IDiscoveryCacheAccessor, DiscoveryCacheAccessor>();
            services.AddSingleton<IIdentityTokenValidator, IdentityTokenValidator>();
 
            return services;
        }
        public static IServiceCollection AddTenantServices(this IServiceCollection services)
        {
            services.AddTransient<TenantContext>();
            return services;
        }
        public static IServiceCollection AddScopedServices(this IServiceCollection services)
        {
            services.AddScopedContex();
            services.AddScoped<IScopedHttpContextRequestForm, ScopedHttpContextRequestForm>();
            services.AddScoped<IScopedOptionalClaims, ScopedOptionalClaims>();
            services.AddScoped<IScopedOverrideRawScopeValues, ScopedOverrideRawScopeValues>();
            return services;
        }

      

        public static IServiceCollection SwapOutDeviceCodeValidator<T>(
            this IServiceCollection services) where T : class, IDeviceCodeValidator
        {

            services.RemoveAll<IDeviceCodeValidator>();
            services.AddTransient<IDeviceCodeValidator, T>();
            return services;
        }
        public static IServiceCollection SwapOutCustomTokenRequestValidator<T>(
            this IServiceCollection services) where T : class, ICustomTokenRequestValidator
        {

            services.RemoveAll<ICustomTokenRequestValidator>();
            services.AddTransient<ICustomTokenRequestValidator, T>();
            return services;
        }
        public static IServiceCollection SwapOutClientSecretValidator<T>(
            this IServiceCollection services) where T : class, IClientSecretValidator
        {

            services.RemoveAll<IClientSecretValidator>();
            services.AddTransient<IClientSecretValidator, T>();
            return services;
        }
        public static IServiceCollection SwapOutIntrospectionResponseGenerator<T>(
            this IServiceCollection services) where T : class, IIntrospectionResponseGenerator
        {

            services.RemoveAll<IIntrospectionResponseGenerator>();
            services.AddTransient<IIntrospectionResponseGenerator, T>();
            return services;
        }

        public static IIdentityServerBuilder SwapOutTokenService<T>(
            this IIdentityServerBuilder builder) where T : class, ITokenService
        {
            builder.Services.SwapOutTokenService<T>();
            return builder;
        }

        public static IServiceCollection SwapOutTokenService<T>(
            this IServiceCollection services) where T : class, ITokenService
        {

            services.RemoveAll<ITokenService>();
            services.AddTransient<ITokenService, T>();
            return services;
        }

        public static IServiceCollection AddClaimsService<T>(this IServiceCollection services)
            where T : class, IClaimsService
        {
            services.AddTransient<IClaimsService, T>();
            return services;
        }

        public static IServiceCollection AddInMemoryTenantAwarePersistedGrantStoreOperationalStore(
            this IServiceCollection services)
        {
            services.AddScoped<IPersistedGrantStoreEx, InMemoryTenantAwarePersistedGrantStore>();
            services.AddScoped<IPersistedGrantStore>(sp =>
            {
                var persistedGrantStoreEx = sp.GetRequiredService<IPersistedGrantStoreEx>();
                return persistedGrantStoreEx;
            });
            return services;
        }
    }
}
