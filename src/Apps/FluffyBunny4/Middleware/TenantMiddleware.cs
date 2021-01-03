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
using FluffyBunny4.DotNetCore.Services;
using FluffyBunny4.Models;

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
            IScopedContext<TenantContext> scopedTenantContext,
            ITenantStore tenantStore)
        {
            try
            {
                // TODO: Probably should use regex here.
                string[] parts = context.Request.Path.Value.Split('/');
                if (parts.Count() > 1)
                {
                    string tenantName = parts[1];
                    scopedTenantContext.Context.TenantName = tenantName;
                    if (string.IsNullOrWhiteSpace(tenantName) || !await tenantStore.IsTenantValidAsync(tenantName))
                    {
                        _logger.LogWarning($"TenantId={tenantName}, does not exist!");
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
                    context.Request.PathBase = $"/{tenantName}";
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
