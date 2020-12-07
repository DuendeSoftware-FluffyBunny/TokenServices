using FluffyBunny4.Extensions;
using FluffyBunny4.Services;
using FluffyBunny4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FluffyBunny4.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context, 
            IScopedTenantRequestContext scopedTenantRequestContext, 
            ITenantStore tenantStore)
        {
            try
            {
                // TODO: Probably should use regex here.
                string[] parts = context.Request.Path.Value.Split('/');
                if (parts.Count() > 1)
                {
                    string tenantId = parts[1];
                    scopedTenantRequestContext.TenantId = tenantId;
                    if (string.IsNullOrWhiteSpace(tenantId) || !await tenantStore.IsTenantValidAsync(tenantId))
                    {
                        _logger.LogWarning($"TenantId={tenantId}, does not exist!");
                        context.Response.Clear();
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        await context.Response.WriteAsync("");
                        return;
                    }
                    StringBuilder sb = new StringBuilder();
                    sb.Append('/');
                    if (parts.Count() > 2)
                    {
                        for (int i = 2; i < parts.Count(); i++)
                        {

                            sb.Append(parts[i]);
                            if (i < parts.Count() - 1)
                            {
                                sb.Append('/');
                            }
                        }
                    }
                    string newPath = sb.ToString();
                    context.Request.Path = newPath;
                    context.Request.PathBase = $"/{tenantId}";
                }

            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unhandled exception: {exception}", ex.Message);
                throw;
            }

            await _next(context);
        }
    }
}
