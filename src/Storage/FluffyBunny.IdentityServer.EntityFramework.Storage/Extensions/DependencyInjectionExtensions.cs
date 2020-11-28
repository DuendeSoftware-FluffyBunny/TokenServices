using System;
using System.Collections.Generic;
using System.Text;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.EntityFramework.Options;
using FluffyBunny.IdentityServer.EntityFramework.Storage.DbContexts;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
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
            return services;
        }
    }
}
