using System;
using System.Collections.Generic;
using System.Text;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.EntityFramework.Options;
using Duende.IdentityServer.Stores;
using FluffyBunny.IdentityServer.EntityFramework.Storage.AutoMapper;
using FluffyBunny.IdentityServer.EntityFramework.Storage.DbContexts;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddSqlServerDbContextOptionsProvider(
            this IServiceCollection services)
        {
            services.AddSingleton<IDbContextOptionsProvider, SqlServerDbContextOptionsProvider>();
            return services;
        }
        public static IServiceCollection AddPostgresDbContextOptionsProvider(
            this IServiceCollection services)
        {
            services.AddSingleton<IDbContextOptionsProvider, PostgresDbContextOptionsProvider>();
            return services;
        }
        public static IServiceCollection AddCosmosDbContextOptionsProvider(
            this IServiceCollection services)
        {
            services.AddSingleton<IDbContextOptionsProvider, CosmosDbContextOptionsProvider>();
            return services;
        }
        public static IServiceCollection AddInMemoryDbContextOptionsProvider(
            this IServiceCollection services)
        {
            services.AddSingleton<IDbContextOptionsProvider, InMemoryDbContextOptionsProvider>();
            return services;
        }

        public static IIdentityServerBuilder AddEntityFrameworkStores(
            this IIdentityServerBuilder builder)
        {
            builder.AddEntityFrameworkResourceStore();
            builder.AddEntityFrameworkClientStore();
            builder.AddEntityFrameworkTenantStore();
            builder.AddEntityFrameworkExternalServicesStore();
            return builder;
        }
        public static IIdentityServerBuilder AddEntityFrameworkExternalServicesStore(
            this IIdentityServerBuilder builder)
        {
            builder.Services.AddEntityFrameworkExternalServicesStore();
            return builder;
        }
        public static IServiceCollection AddEntityFrameworkExternalServicesStore(
            this IServiceCollection services)
        {
            services.AddExternalServicesStoreCache<EntityFrameworkExternalServicesStore>();
            return services;
        }
        public static IIdentityServerBuilder AddEntityFrameworkResourceStore(
            this IIdentityServerBuilder builder)
        {
            return builder.AddResourceStoreCache<EntityFrameworkResourceStore>();
        }
        public static IServiceCollection AddEntityFrameworkResourceStore(
            this IServiceCollection services)
        {
            return services.AddResourceStoreCache<EntityFrameworkResourceStore>();
        }
        public static IServiceCollection AddResourceStoreCache<T>(this IServiceCollection services)
            where T : IResourceStore
        {
            services.TryAddTransient(typeof(T));
            services.AddTransient<IResourceStore, CachingResourceStore<T>>();
            return services;
        }
        public static IIdentityServerBuilder AddEntityFrameworkTenantStore(
            this IIdentityServerBuilder builder)
        {
            builder.Services.AddEntityFrameworkTenantStore();
            return builder;
        }
        public static IServiceCollection AddEntityFrameworkTenantStore(
            this IServiceCollection services)
        {
            services.AddTenantStoreCache<EntityFrameworkTenantStore>();
            return services;
        }
        public static IIdentityServerBuilder AddEntityFrameworkClientStore(
            this IIdentityServerBuilder builder)
        {
            return builder.AddClientStoreCache<EntityFrameworkClientStore>();
        }
        public static IServiceCollection AddEntityFrameworkClientStore(
            this IServiceCollection services)
        {
            return services.AddClientStoreCache<EntityFrameworkClientStore>();
        }
        public static IServiceCollection AddClientStoreCache<T>(this IServiceCollection services)
            where T : IClientStore
        {
            services.TryAddTransient(typeof(T));
            services.AddTransient<IClientStore, CachingClientStore<T>>();
            return services;
        }

        public static IServiceCollection AddDbContextTenantServices(this IServiceCollection services)
        {
            services.AddScoped<IAdminServices, AdminServices>();
            services.TryAddSingleton<ITenantAwareConfigurationDbContextAccessor, TenantAwareConfigurationDbContextAccessor>();
            services.AddDbContext<TenantAwareConfigurationDbContext>((serviceProvider, optionsBuilder) => {
                // for NON-INMEMORY  - TenantAwareConfigurationDbContext
                // this is only here so that migration models can be created.
                // we then use it as a template to not only create the new database for the tenant, but
                // downstream using it as a normal connection.

                var dbContextOptionsProvider = serviceProvider.GetRequiredService<IDbContextOptionsProvider>();
                dbContextOptionsProvider.Configure(optionsBuilder);
            });
            services.AddScoped<IMainEntityCoreContext, MainEntityCoreContext>();
            services.AddDbContext<MainEntityCoreContext>((serviceProvider, optionsBuilder) => {
                var dbContextOptionsProvider = serviceProvider.GetRequiredService<IDbContextOptionsProvider>();
                dbContextOptionsProvider.Configure(optionsBuilder);
            });

            var mapperOneToOne = MapperConfigurationBuilder.BuidOneToOneMapper;
            var mapperIgnoreBase = MapperConfigurationBuilder.BuidIgnoreBaseMapper;
            services.AddSingleton<IEntityFrameworkMapperAccessor>(new EntityFrameworkMapperAccessor
            {
                MapperOneToOne = mapperOneToOne,
                MapperIgnoreBase = mapperIgnoreBase
            });

            return services;
        }
    }
}
