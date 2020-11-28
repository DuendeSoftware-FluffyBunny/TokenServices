using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.Admin.Services;

namespace Microsoft.AspNetCore.Identity
{
    public class EnsureSessionTenantMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly AuthSessionValidationOptions _options;
        private readonly ILogger<AuthSessionValidationMiddleware> _logger;

        public EnsureSessionTenantMiddleware(
            RequestDelegate next,
            IOptions<AuthSessionValidationOptions> options,
            ILogger<AuthSessionValidationMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options.Value;
            _logger = logger;
        }

        public async Task Invoke(
            HttpContext context, 
            IServiceProvider serviceProvider,
            ISessionTenantAccessor sessionTenantAccessor)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (context.User.Identity.IsAuthenticated)
            {
                if (context.Request.Path.StartsWithSegments("/Tenant"))
                {
                    var tenantId = sessionTenantAccessor.TenantId;
                    if (string.IsNullOrWhiteSpace(tenantId))
                    {
                        context.Response.Redirect("/Tenants/Index");
                        return;
                    }

                }
            }
            await _next(context);
        }
    }
}
