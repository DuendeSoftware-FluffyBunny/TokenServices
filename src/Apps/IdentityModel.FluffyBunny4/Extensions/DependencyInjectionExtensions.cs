using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityModel.FluffyBunny4.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddFluffyBunnyTokenService(this IServiceCollection services)
        {
            services.AddSingleton<IFluffyBunnyTokenService, FluffyBunnyTokenService>();
            return services;
        }
    }
}
