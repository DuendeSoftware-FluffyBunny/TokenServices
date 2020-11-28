using FluffyBunny4.Middleware;
using Microsoft.AspNetCore.Builder;

namespace FluffyBunny4.Configuration
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseTenantServices(
            this IApplicationBuilder app)
        {
            app.UseMiddleware<TenantMiddleware>();
            return app;
        }
    }
}
