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
        public static IServiceCollection AddPostgresDbContextOptionsProvider(
            this IServiceCollection services)
        {
            services.AddSingleton<IDbContextOptionsProvider, PostgresDbContextOptionsProvider>();
            return services;
        }
    }
}
