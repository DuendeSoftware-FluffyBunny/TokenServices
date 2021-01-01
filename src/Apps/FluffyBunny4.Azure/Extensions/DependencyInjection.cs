using AutoMapper;
using FluffyBunny4.Azure.Clients;
using FluffyBunny4.Azure.Configuration.CosmosDB;
using FluffyBunny4.Azure.DbContext;
using FluffyBunny4.Azure.KeyVault;
using FluffyBunny4.Azure.Models;
using FluffyBunny4.Azure.Services;
using FluffyBunny4.Azure.Stores.CosmosDB;
using FluffyBunny4.Azure.Utils;
using FluffyBunny4.Configuration;
using FluffyBunny4.DotNetCore.Extensions;
using FluffyBunny4.Extensions;
using FluffyBunny4.Models;
using FluffyBunny4.Services;
using FluffyBunny4.Stores;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddTenantAwareOperationalCosmosDBRepository(this IServiceCollection services)
        {
            services.AddScoped<ISimpleItemDbContext<PersistedGrantCosmosDocument>, TenantAwareOperationalCosmosDBRepository>();
            return services;
        }

        public static IServiceCollection AddAzureClients(this IServiceCollection services)
        {
            services.AddSingleton<KeyVaultClient>(sp =>
            {
                return ManagedIdentityHelper.CreateKeyVaultClient();
            });
            services.AddSingleton<AzureKeyVaultTokenCredential>();
            services.AddSingleton<IAzureKeyVaultClients, AzureKeyVaultClients>();
            services.AddSingleton<IKeyVaultSecretProvider, KeyVaultSecretProvider>();

            return services;
        }

        public static IServiceCollection AddKeyVaultTokenCreationServices(this IServiceCollection services)
        {
            services.AddSerializers();
            services.AddAzureClients();
            services.RemoveAll<ITokenCreationService>();
            services.AddTransient<ITokenCreationService, KeyVaultTokenCreationService>();
            return services;
        }
        public static IServiceCollection AddTenantResolverCache<T>(this IServiceCollection services)
            where T : IKeyVaultTenantResolver
        {
            services.TryAddTransient(typeof(T));
            services.AddTransient<ITenantResolver, CachingTenantResolver<T>>();
            services.AddTransient<IKeyVaultTenantResolver, CachingTenantResolver<T>>();
            return services;
        }
         
        
        public static IServiceCollection AddKeyVaultECDsaStores(
            this IServiceCollection services,
            Action<KeyVaultStoreOptions> options)
        {
            services.AddTransient<KeyVaultECDsaKeyStore>();
            services.AddScoped<IJwksDiscovery>(sp =>
            {
                var implementation = sp.GetRequiredService<KeyVaultECDsaKeyStore>();
                return implementation;
            });
            services.AddScoped<IKeyVaultECDsaKeyStore>(sp =>
            {
                var implementation = sp.GetRequiredService<KeyVaultECDsaKeyStore>();
                return implementation;
            });
            services.AddScoped<IValidationKeysStore, TenantValidationKeyStore>();

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options),
                    @"Please provide options for KeyVaultStoreOptions.");
            }
            services.Configure(options);
            return services;
        }
        public static IServiceCollection AddKeyVaultCertificatesStores(this IServiceCollection services,
            Action<KeyVaultStoreOptions> options)
        {
            services.AddSingleton<KeyVaultCertificateStore>();
            services.AddSingleton<IJwksDiscovery>(sp =>
            {
                var implementation = sp.GetRequiredService<KeyVaultCertificateStore>();
                return implementation;
            });
            services.AddSingleton<IKeyVaultCertificateStore>(sp =>
            {
                var implementation = sp.GetRequiredService<KeyVaultCertificateStore>();
                return implementation;
            });

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options),
                    @"Please provide options for KeyVaultStoreOptions.");
            }
            services.Configure(options);
            return services;
        }
        public static IServiceCollection AddAzureKeyVaultCertificateSignatureProvider(
            this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ISignatureProvider, AzureKeyVaultSignatureProvider>();
            return serviceCollection;
        }
        public static IServiceCollection AddAzureKeyVaultECDsaSignatureProvider(
           this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<AzureKeyVaultECDsaSignatureProvider>();
            serviceCollection.AddTransient<ISignatureProvider, AzureKeyVaultECDsaSignatureProvider>();
            return serviceCollection;
        }
       
        public static IServiceCollection AddCosmosOperationalStore(
            this IServiceCollection services)
        {
            services.AddTenantAwareOperationalCosmosDBRepository();
            services.AddScoped<IPersistedGrantStoreEx, PersistedGrantStore>();
            services.AddScoped<IPersistedGrantStore>((sp) =>
            {
                var store = sp.GetRequiredService<IPersistedGrantStoreEx>();
                return store;
            });
            return services;
        }
    }
}
