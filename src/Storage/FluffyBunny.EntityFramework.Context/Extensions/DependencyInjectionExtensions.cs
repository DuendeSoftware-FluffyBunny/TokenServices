using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace FluffyBunny.EntityFramework.Context.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddSqlServerDbContextOptionsProvider(
            this IServiceCollection services)
        {
            services.AddSingleton<IDbContextOptionsProvider, SqlServerDbContextOptionsProvider>();
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
    }
}
