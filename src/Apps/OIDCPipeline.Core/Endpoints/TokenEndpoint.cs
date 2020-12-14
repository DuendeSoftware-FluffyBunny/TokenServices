using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
 
using FluffyBunny4.DotNetCore;
using FluffyBunny4.DotNetCore.Extensions;
using FluffyBunny4.DotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OIDCPipeline.Core.Configuration;
using OIDCPipeline.Core.Endpoints.ResponseHandling;
using OIDCPipeline.Core.Endpoints.Results;
using OIDCPipeline.Core.Hosting;
using OIDCPipeline.Core.Validation;

namespace OIDCPipeline.Core.Endpoints
{
    internal class TokenEndpoint : IEndpointHandler
    {
        private OIDCPipelineOptions _options;
        private IOIDCPipelineStore _oidcPipelineStore;
        private ITokenRequestValidator _tokenRequestValidator;
        private ISerializer _serializer;
        private ILogger<AuthorizeEndpoint> _logger;

        public TokenEndpoint(
            OIDCPipelineOptions options,
            IOIDCPipelineStore oidcPipelineStore,
            ITokenRequestValidator tokenRequestValidator,
            ISerializer serializer,
            ILogger<AuthorizeEndpoint> logger)
        {
            _options = options;
            _oidcPipelineStore = oidcPipelineStore;
            _tokenRequestValidator = tokenRequestValidator;
            _serializer = serializer;
            _logger = logger;
        }
        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            // we only support code authorization, the rest is dropped on the floor
            try
            {
                SimpleNameValueCollection values;
                if (HttpMethods.IsGet(context.Request.Method))
                {
                    values = context.Request.Query.AsNameValueCollection();
                }
                else if (HttpMethods.IsPost(context.Request.Method))
                {
                    if (!context.Request.HasFormContentType)
                    {
                        return new OIDCPipeline.Core.Endpoints.Results.StatusCodeResult(StatusCodes.Status415UnsupportedMediaType);
                    }

                    values = context.Request.Form.AsNameValueCollection();
                }
                else
                {
                    return new OIDCPipeline.Core.Endpoints.Results.StatusCodeResult((int)HttpStatusCode.MethodNotAllowed);
                }
                var validatedResult = await _tokenRequestValidator.ValidateRequestAsync(values);
                if (validatedResult.IsError)
                {
                    throw new Exception(validatedResult.ErrorDescription);
                }
                var downstream = validatedResult.Request.IdTokenResponse;

                var tokenResponse = new TokenResponse()
                {
                    IdentityToken = downstream.IdToken,
                    AccessToken = downstream.AccessToken,
                    AccessTokenLifetime = Convert.ToInt32(downstream.ExpiresAt),
                    Custom = downstream.Custom
                };
                var result = new TokenResult(tokenResponse, _serializer);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
                return new OIDCPipeline.Core.Endpoints.Results.StatusCodeResult((int)StatusCodes.Status404NotFound);
            }
        }
    }
}
