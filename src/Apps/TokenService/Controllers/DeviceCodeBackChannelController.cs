using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.ResponseHandling;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using FluffyBunny4;
using FluffyBunny4.Cache;
using FluffyBunny4.Configuration;
using FluffyBunny4.DotNetCore.Services;
using FluffyBunny4.Models;
using FluffyBunny4.Services;
using FluffyBunny4.Stores;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TokenService.Models;

namespace TokenService.Controllers
{
    [ApiController]
    [Route("{tenantId}/[controller]")]
    public class DeviceCodeBackChannelController : ControllerBase
    {
        private ILogger<DeviceCodeBackChannelController> _logger;
        private IExternalServicesStore _externalServicesStore;
        private TokenExchangeOptions _tokenExchangeOptions;
        private IIdentityTokenValidator _identityTokenValidator;
        private IConsentExternalService _consentExternalService;
        private IConsentDiscoveryCacheAccessor _consentDiscoveryCacheAccessor;
        private IClientSecretValidator _clientValidator;
        private IDeviceFlowStore _deviceFlowStore;
        private ISerializer _serializer;
        private ICoreMapperAccessor _coreMapperAccessor;
        private IEventService _events;

        public DeviceCodeBackChannelController(
            IExternalServicesStore externalServicesStore,
            IOptions<TokenExchangeOptions> tokenExchangeOptions,
            IIdentityTokenValidator identityTokenValidator,
            IConsentExternalService consentExternalService,
            IConsentDiscoveryCacheAccessor consentDiscoveryCacheAccessor,
            IClientSecretValidator clientValidator,
            IDeviceFlowStore deviceFlowStore,
            ISerializer serializer,
            ICoreMapperAccessor coreMapperAccessor,
            IEventService events, 
            ILogger<DeviceCodeBackChannelController> logger)
        {
            _externalServicesStore = externalServicesStore;
            _tokenExchangeOptions = tokenExchangeOptions.Value;
            _identityTokenValidator = identityTokenValidator;
            _consentExternalService = consentExternalService;
            _consentDiscoveryCacheAccessor = consentDiscoveryCacheAccessor;
            _clientValidator = clientValidator;
            _deviceFlowStore = deviceFlowStore;
            _serializer = serializer;
            _coreMapperAccessor = coreMapperAccessor;
            _events = events;
            _logger = logger;
        }

 
        [HttpGet]
        [Route("device-code-by-user-code")]
        public async Task<ActionResult<DeviceCode>> GetDeviceCodeByUserCodeAsync(string userCode)
        {
            // validate client
            var clientResult = await _clientValidator.ValidateAsync(this.HttpContext);
            if (clientResult.Client == null) return Unauthorized(OidcConstants.TokenErrors.InvalidClient);

            var grantType = clientResult.Client.AllowedGrantTypes.FirstOrDefault(agt=>agt == OidcConstants.GrantTypes.DeviceCode);
            if (grantType == null) return Unauthorized(OidcConstants.TokenErrors.InvalidGrant);
            
            var deviceCode = await _deviceFlowStore.FindByUserCodeAsync(userCode.Sha256());
            if (deviceCode == null)
            {
                return NotFound();
            }
            return deviceCode;
        }
        [HttpGet]
        [Route("device-code-by-device-code")]
        public async Task<ActionResult<DeviceCode>> GetDeviceCodeByDeviceCodeAsync(string deviceCode)
        {
            // validate client
            var clientResult = await _clientValidator.ValidateAsync(this.HttpContext);
            if (clientResult.Client == null) return Unauthorized(OidcConstants.TokenErrors.InvalidClient);

            var grantType = clientResult.Client.AllowedGrantTypes.FirstOrDefault(agt => agt == OidcConstants.GrantTypes.DeviceCode);
            if (grantType == null) return Unauthorized(OidcConstants.TokenErrors.InvalidGrant);

            var result = await _deviceFlowStore.FindByDeviceCodeAsync(deviceCode);
            if (result == null)
            {
                return NotFound();
            }
            return result;
        }

        public class AuthorizeDeviceRequest
        {
            public string IdToken { get; set; }
            public string UserCode { get; set; }
            public string Issuer { get; set; }
        }

        string SubjectFromClaimsPrincipal(ClaimsPrincipal principal)
        {
            var subjectClaim = principal.Claims.FirstOrDefault(a => a.Type == JwtClaimTypes.Subject);
            if (subjectClaim != null)
            {
                return subjectClaim.Value;
            }
            subjectClaim = principal.Claims.FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier);
            if (subjectClaim != null)
            {
                return subjectClaim.Value;
            }

            return null;

        }
        Dictionary<string, List<string>> GetServiceToScopesFromRequest(List<string> requestedScopes)
        {
            var result = new Dictionary<string, List<string>>();
            var index = _tokenExchangeOptions.BaseScope.Length;
            var requestedServiceScopes = (from item in requestedScopes
                where item.StartsWith(_tokenExchangeOptions.BaseScope)
                select item.Substring(index)).ToList();
            foreach (var item in requestedServiceScopes)
            {
                var parts = item.Split('.');
                if (!result.ContainsKey(parts[0]))
                {
                    result[parts[0]] = new List<string>();
                }
                result[parts[0]].Add($"{_tokenExchangeOptions.BaseScope}{item}");
            }
            return result;
        }

        [HttpPost]
        [Route("authorize-device")]
        public async Task<IActionResult> AuthorizeDeviceAsync([FromBody] AuthorizeDeviceRequest data)
        {
            // validate client
            var clientResult = await _clientValidator.ValidateAsync(this.HttpContext);
            if (clientResult.Client == null) return Unauthorized(OidcConstants.TokenErrors.InvalidClient);
            var client = clientResult.Client as ClientExtra;

            var grantType =
                clientResult.Client.AllowedGrantTypes.FirstOrDefault(agt => agt == OidcConstants.GrantTypes.DeviceCode);
            if (grantType == null) return Unauthorized(OidcConstants.TokenErrors.InvalidGrant);

            var deviceAuth = await _deviceFlowStore.FindByUserCodeAsync(data.UserCode.Sha256());
            if (deviceAuth == null)
                return NotFound($"Invalid user code, Device authorization failure - user code is invalid");


            // VALIDATE issuer must exist and must be allowed
            // -------------------------------------------------------------------
            var issuer = data.Issuer;
            if (string.IsNullOrEmpty(data.Issuer))
            {
                return NotFound($"Issuer is required");
            }
            else
            {
                issuer = issuer.ToLower();
                var foundIssuer = client.AllowedArbitraryIssuers.FirstOrDefault(x => x == issuer);
                if (string.IsNullOrWhiteSpace(foundIssuer))
                {
                    return NotFound($"issuer:{issuer} is NOT in the AllowedArbitraryIssuers collection.");
                }
            }

            string subject = "";
            try
            {
                var validatedResult =
                    await _identityTokenValidator.ValidateIdTokenAsync(data.IdToken,
                        _tokenExchangeOptions.AuthorityKey);
                if (validatedResult.IsError)
                {
                    throw new Exception(
                        $"failed to validate: {FluffyBunny4.Constants.TokenExchangeTypes.SubjectToken}={data.IdToken}",
                        new Exception(validatedResult.Error));
                }

                subject = SubjectFromClaimsPrincipal(validatedResult.User);
                if (string.IsNullOrWhiteSpace(subject))
                {
                    throw new Exception(
                        $"subject does not exist: {FluffyBunny4.Constants.TokenExchangeTypes.SubjectToken}={data.IdToken}");
                }

                var claims = validatedResult.User.Claims.ToList();
                claims.Add(new Claim(JwtClaimTypes.AuthenticationTime,
                    claims.FirstOrDefault(c => c.Type == JwtClaimTypes.IssuedAt).Value));
                claims.Add(new Claim(JwtClaimTypes.IdentityProvider, _tokenExchangeOptions.AuthorityKey));

                var newClaimsIdentity = new ClaimsIdentity(claims);
                var subjectPrincipal = new ClaimsPrincipal(newClaimsIdentity);

                deviceAuth.Subject = subjectPrincipal;

                var requestedScopes = deviceAuth.RequestedScopes.ToList();
                var offlineAccess =
                    requestedScopes.FirstOrDefault(x => x == IdentityServerConstants.StandardScopes.OfflineAccess);

                var allowedScopes = new List<string>();
                var allowedClaims = new List<Claim>();
                var finalCustomPayload = new Dictionary<string, object>();
                var requestedServiceScopes = GetServiceToScopesFromRequest(deviceAuth.RequestedScopes.ToList());
                foreach (var serviceScopeSet in requestedServiceScopes)
                {
                    var externalService =
                        await _externalServicesStore.GetExternalServiceByNameAsync(serviceScopeSet.Key);
                    if (externalService == null)
                    {
                        continue;
                    }

                    var discoCache =
                        await _consentDiscoveryCacheAccessor.GetConsentDiscoveryCacheAsync(serviceScopeSet.Key);
                    var doco = await discoCache.GetAsync();

                    List<string> scopes = null;
                    switch (doco.AuthorizationType)
                    {
                        case Constants.AuthorizationTypes.Implicit:
                            scopes = null;
                            break;
                        case Constants.AuthorizationTypes.SubjectAndScopes:
                            scopes = serviceScopeSet.Value;
                            break;
                    }

                    if (doco.AuthorizationType == Constants.AuthorizationTypes.Implicit)
                    {
                        allowedScopes.AddRange(serviceScopeSet.Value);
                    }
                    else
                    {
                        var request = new ConsentAuthorizeRequest
                        {
                            AuthorizeType = doco.AuthorizationType,
                            Scopes = scopes,
                            Subject = subject
                        };
                        var response = await _consentExternalService.PostAuthorizationRequestAsync(doco, request);
                        if (response.Authorized)
                        {
                            switch (doco.AuthorizationType)
                            {

                                case Constants.AuthorizationTypes.SubjectAndScopes:
                                    // make sure no funny business is coming in from the auth call.
                                    var serviceRoot = $"{_tokenExchangeOptions.BaseScope}{serviceScopeSet.Key}";
                                    var query = (from item in response.Scopes
                                        where item.StartsWith(serviceRoot)
                                        select item);
                                    allowedScopes.AddRange(query);
                                    if (response.Claims != null && response.Claims.Any())
                                    {
                                        foreach (var cac in response.Claims)
                                        {
                                            // namespace the claims.
                                            allowedClaims.Add(new Claim($"{serviceScopeSet.Key}.{cac.Type}",
                                                cac.Value));
                                        }
                                    }

                                    if (response.CustomPayload != null)
                                    {
                                        finalCustomPayload.Add(serviceScopeSet.Key, response.CustomPayload);
                                    }

                                    break;
                            }
                        }
                    }
                }

                if (finalCustomPayload.Any())
                {
                    allowedClaims.Add(new Claim(
                        Constants.CustomPayload,
                        _serializer.Serialize(finalCustomPayload),
                        IdentityServerConstants.ClaimValueTypes.Json));
                }

                deviceAuth.IsAuthorized = true;
                //  deviceAuth.SessionId = sid;
                if (!string.IsNullOrWhiteSpace(offlineAccess))
                {
                    allowedScopes.Add(offlineAccess);
                }

                deviceAuth.AuthorizedScopes = allowedScopes;
                var deviceExtra = _coreMapperAccessor.Mapper.Map<DeviceCodeExtra>(deviceAuth);
                deviceExtra.AuthorizedClaims = allowedClaims;
                deviceExtra.Issuer = issuer;

                await _deviceFlowStore.UpdateByUserCodeAsync(data.UserCode.Sha256(), deviceExtra);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500);
            }


        }

      
    }
}