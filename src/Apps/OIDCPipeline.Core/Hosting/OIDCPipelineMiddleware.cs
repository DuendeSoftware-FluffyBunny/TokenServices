using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OIDCPipeline.Core.Hosting
{
    class ResponseState
    {
        public HttpContext HttpContext { get; set; }
        public Dictionary<string, string> CookieItems { get; set; }
        public IEndpointResult EndpointResult { get; internal set; }
    }
    internal class OIDCPipelineMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public OIDCPipelineMiddleware(RequestDelegate next, ILogger<OIDCPipelineMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(
            HttpContext context,
            IEndpointRouter router)
        {
            try
            {
                var endpoint = router.Find(context);
                if (endpoint != null)
                {
                    _logger.LogInformation("Invoking IdentityServer endpoint: {endpointType} for {url}", endpoint.GetType().FullName, context.Request.Path.ToString());

                    var result = await endpoint.ProcessAsync(context);

                    if (result != null)
                    {
                        
                        _logger.LogTrace("Invoking result: {type}", result.GetType().FullName);
                        var state = new ResponseState
                        {
                            EndpointResult = result,
                            HttpContext = context,
                            CookieItems = new Dictionary<string, string>()
                        };
                      
                        state.CookieItems["herb"] = "stahl";
                        context.Response.OnStarting(OnStartingCallBack,state);
                    }

                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unhandled exception: {exception}", ex.Message);
                throw;
            }

            await _next(context);
        }
        private async Task OnStartingCallBack(object state)
        {
            var responseState = state as ResponseState;
            var cookieOptions = new CookieOptions()
            {
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddHours(1),
                IsEssential = true,
                HttpOnly = false,
                Secure = false,
            };
            foreach(var item in responseState.CookieItems)
            {
                responseState.HttpContext.Response.Cookies.Append(item.Key, item.Value, cookieOptions);
             
            }
            await responseState.EndpointResult.ExecuteAsync(responseState.HttpContext);

            

        }
    }
}
