using FluffyBunny4.DotNetCore.Collections;
using FluffyBunny4.DotNetCore.Hosting;
using FluffyBunny4.DotNetCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using FluffyBunny4.DotNetCore.Services.Defaults;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddNullDataProtection(this IServiceCollection services)
        {
            services.AddSingleton<IDataProtection, NullDataProtection>();
            return services;
        }

        public static IServiceCollection AddDataProtection(this IServiceCollection services)
        {
            services.AddSingleton<IDataProtection, DataProtection>();
            return services;
        }
        public static IServiceCollection AddScopedContex(this IServiceCollection services)
        {
            services.TryAddScoped(typeof(IScopedContext<>), typeof(ScopedContext<>));
            return services;
        }
        public static IServiceCollection AddScopedStorage(this IServiceCollection services)
        {
            services.TryAddScoped<IScopedStorage, ThreadSafeScopedStorage>();
            return services;
        }
        public static IServiceCollection AddHostStorage(this IServiceCollection services)
        {
            services.TryAddSingleton<IHostStorage, ThreadSafeHostStorage>();
            return services;
        }
        public static IServiceCollection AddSerializers(this IServiceCollection services)
        {
            services.AddSingleton<IBinarySerializer, BinarySerializer>();
            services.AddSingleton<ISerializer, Serializer>();
            return services;
        }
        public static IServiceCollection AddBackgroundServices<T>(this IServiceCollection services)
            where T : class
        {
            services.AddHostedService<QueuedHostedService<T>>();
            services.AddSingleton(typeof(IBackgroundTaskQueue<>), typeof(BackgroundTaskQueue<>));
            return services;
        }
    }
}
