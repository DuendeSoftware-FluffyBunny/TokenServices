using FluffyBunny4.DotNetCore.Collections;
using FluffyBunny4.DotNetCore.Hosting;
using FluffyBunny4.DotNetCore.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddHostStorage(this IServiceCollection services)
        {
            services.AddSingleton<IHostStorage, ThreadSafeHostStorage>();
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
