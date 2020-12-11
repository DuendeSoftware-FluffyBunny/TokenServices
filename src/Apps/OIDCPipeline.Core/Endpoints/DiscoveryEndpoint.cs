using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OIDC.ReferenceWebClient.Discovery;
using OIDCPipeline.Core.Configuration;
using OIDCPipeline.Core.Endpoints.Results;
using OIDCPipeline.Core.Hosting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OIDCPipeline.Core.Endpoints
{
    internal class DiscoveryEndpoint : IEndpointHandler
    {
        private readonly ILogger _logger;
        private readonly IDownstreamDiscoveryCache _downstreamDiscoveryCache;
        private readonly OIDCPipelineOptions _options;
        public DiscoveryEndpoint(
           OIDCPipelineOptions options,
           IDownstreamDiscoveryCache downstreamDiscoveryCache,
           ILogger<DiscoveryEndpoint> logger)
        {
            _logger = logger;
            _downstreamDiscoveryCache = downstreamDiscoveryCache;
            _options = options;
            
        }
        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            _logger.LogTrace("Processing discovery request.");
            // validate HTTP
            if (!HttpMethods.IsGet(context.Request.Method))
            {
                _logger.LogWarning("Discovery endpoint only supports GET requests");
                return new OIDCPipeline.Core.Endpoints.Results.StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }
            _logger.LogDebug("Start discovery request");

            var response = await _downstreamDiscoveryCache.GetAsync();
            var downstreamStuff = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Raw);
            downstreamStuff["authorization_endpoint"]
               = $"{context.Request.Scheme}://{context.Request.Host}/connect/authorize";
            downstreamStuff["token_endpoint"]
              = $"{context.Request.Scheme}://{context.Request.Host}/connect/token";
            return new DiscoveryDocumentResult(downstreamStuff, _options.Discovery.ResponseCacheInterval);

        }
    }
}
