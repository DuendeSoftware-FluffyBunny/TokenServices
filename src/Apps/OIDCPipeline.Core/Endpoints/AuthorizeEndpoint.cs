using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common;
using IdentityModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OIDCPipeline.Core.Configuration;
using OIDCPipeline.Core.Endpoints.Results;
using OIDCPipeline.Core.Extensions;
using OIDCPipeline.Core.Hosting;
using OIDCPipeline.Core.Logging;
using OIDCPipeline.Core.Validation;
using OIDCPipeline.Core.Validation.Models;
using FluffyBunny4.DotNetCore.Extensions;
using FluffyBunny4.DotNetCore;
using Microsoft.AspNetCore.Identity;

namespace OIDCPipeline.Core.Endpoints
{
    internal class AuthorizeEndpoint : IEndpointHandler
    {
        private OIDCPipelineOptions _options;
        private ISigninManager _signinManager;
        private IOIDCPipelineStore _oidcPipelineStore;
        private IAuthorizeRequestValidator _authorizeRequestValidator;
        private IOIDCPipeLineKey _oidcPipeLineKey;
        private ILogger<AuthorizeEndpoint> _logger;

        public AuthorizeEndpoint(
            OIDCPipelineOptions options,
            ISigninManager signinManager,
            IOIDCPipelineStore oidcPipelineStore,
            IAuthorizeRequestValidator authorizeRequestValidator,
            IOIDCPipeLineKey oidcPipeLineKey,
            ILogger<AuthorizeEndpoint> logger)
        {
            _options = options;
            _signinManager = signinManager;
            _oidcPipelineStore = oidcPipelineStore;
            _authorizeRequestValidator = authorizeRequestValidator;
            _oidcPipeLineKey = oidcPipeLineKey;
            _logger = logger;
        }
        internal string GenerateNonce()
        {
            string nonce = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString() + Guid.NewGuid().ToString()));
            return DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture) + "." + nonce;
        }
        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            string key = null;
            try
            {
                
                var request = new ValidatedAuthorizeRequest();

                _logger.LogInformation("Process AuthorizeEndpoint Start.._signinManager.SignOutAsync() ");
                await _signinManager.SignOutAsync();


                /*
                https://localhost:44305/connect/authorize?
    client_id=mvc
    &redirect_uri=https%3A%2F%2Fp7core.127.0.0.1.xip.io%3A44311%2Fsignin-oidc
    &response_type=id_token
    &scope=openid%20profile
    &response_mode=form_post
    &nonce=636973229335838266.ZWJhM2U4M2YtYWNiYi00YjZkLTkwMWYtNjRmMjM3MWRiYTk5OWNkNDIzMWUtZmY4OS00YWE0LTk4MGUtMTdiMjYxNmNmZjRk&state=CfDJ8KOz5LEySMhBtqpccMk4UVhA1PvGQQvpqQBUyR-97TDZvaPuNquTLJIUxKMYzF-Ov_HHCnnmcTForzd5RJ4jmLONvcZLY3XCHnrhh9Sc2oR2Lv2HACvPVBMy2oYmmPBtNIoXroQ9WePE_KtPyFw8ntRsHIYMmT5a0fLKGeJcwK3ewoiRHxjKpOr9hXZau9f7CVVqMvtWC2ngWrFsEeh8S0YtRZQFT-7XyjE9dNiyKp_Z-4iBUbbqzVnT4GmEmErZXUjmBhmVsMLz5h9y_F3usRT3lg7LxUNamnJuROnYIqmJzf0fYVJq1mcB5hcUipo2SNcILG3xkUikc84VznSGvD7V_qFjOHVPtOEX02JH9M4ymb3iZtZSE9dDr2RkwTU7StoKgM-x195bBULpwms8weJO-kx5I6UrY_lmWl0SFqYN
    &x-client-SKU=ID_NETSTANDARD2_0
    &x-client-ver=5.4.0.0

                */

                SimpleNameValueCollection values;
                if (HttpMethods.IsGet(context.Request.Method))
                {
                    values = context.Request.Query.AsNameValueCollection();
                }
                else if (HttpMethods.IsPost(context.Request.Method))
                {
                    if (!context.Request.HasFormContentType)
                    {
                        return new OIDCPipeline.Core.Endpoints.Results.StatusCodeResult((int)HttpStatusCode.UnsupportedMediaType);
                    }

                    values = context.Request.Form.AsNameValueCollection();
                }
                else
                {
                    return new OIDCPipeline.Core.Endpoints.Results.StatusCodeResult((int)HttpStatusCode.MethodNotAllowed);
                }
                request.Raw = values;


                var result = await _authorizeRequestValidator.ValidateAsync(request);
                 
                _logger.LogInformation($"Method:{context.Request.Method} ValidateAsync Error:{result.IsError}");
                string redirectUrl = null;
                if (!result.IsError)
                {
                 
                    if (string.IsNullOrWhiteSpace(request.Nonce))
                    {
                        request.Nonce = GenerateNonce();
                        values["OidcConstants.AuthorizeRequest.Nonce"] = request.Nonce;
                    }
                    var downstreamAuthorizationRequest = values.ToDownstreamAuthorizationRequest();

                    key = _oidcPipeLineKey.GetOIDCPipeLineKey();
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        _logger.LogInformation($"DeleteStoredCacheAsync previouse if it exists");
                        await _oidcPipelineStore.DeleteStoredCacheAsync(key);
                    }

                    // Initially was using the NONCE as the key, but we can't trust that clients will not hardcode a nonce
                    // The key has to be generated on our side and subsequently stored as a cookie.
                    key = Guid.NewGuid().ToString("N");
                  //  context.SetOIDCPipeLineKey(key);

                    _logger.LogInformation($"StoreOriginalIdTokenRequestAsync clientid:{downstreamAuthorizationRequest.client_id}");
                    await _oidcPipelineStore.StoreOriginalAuthorizeRequestAsync(key, result.ValidatedAuthorizeRequest);

                    redirectUrl = $"{context.Request.Scheme}://{context.Request.Host}{_options.PostAuthorizeHookRedirectUrl}";
                }
                else
                {
                    redirectUrl = $"{context.Request.Scheme}://{context.Request.Host}{_options.PostAuthorizeHookErrorRedirectUrl}";
                }
                _logger.LogInformation($"redirecting to:{redirectUrl}");
             

                return new Results.OriginalAuthorizeResult(_oidcPipeLineKey,redirectUrl, key);
            }
            catch (Exception ex)
            {
                string redirectUrl = $"{context.Request.Scheme}://{context.Request.Host}{_options.PostAuthorizeHookErrorRedirectUrl}";
                return new Results.OriginalAuthorizeResult(_oidcPipeLineKey,redirectUrl, key);
            }
        }
         
    }
}
