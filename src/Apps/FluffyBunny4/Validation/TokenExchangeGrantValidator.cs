
using FluffyBunny4.Services;
using FluffyBunny4.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluffyBunny4.Models;
using System.Text.RegularExpressions;
using Duende.IdentityServer;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Mvc;
using FluffyBunny4.Cache;
using FluffyBunny4.Stores;
using Microsoft.Extensions.Options;
using FluffyBunny4.Configuration;
using FluffyBunny4.DotNetCore.Services;
using IdentityModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FluffyBunny4.Validation
{
    public class TokenExchangeGrantValidator : IExtensionGrantValidator
    {
        // private const string _regexExpression = "^[a-zA-Z0-9!@#$%^&*_+=-]*$";

        private const string _regexExpression = @"^[a-zA-Z0-9_\-.:\/]*$";

        private static List<string> _oneMustExitsArguments;
        private IScopedStorage _scopedStorage;
        private ISerializer _serializer;
        private IResourceStore _resourceStore;
        private IScopedOptionalClaims _scopedOptionalClaims;
        private IConsentExternalService _consentExternalService;
        private IExternalServicesStore _externalServicesStore;
        private IScopedOverrideRawScopeValues _scopedOverrideRawScopeValues;
        private IConsentDiscoveryCacheAccessor _consentDiscoveryCacheAccessor;
        private TokenExchangeOptions _tokenExchangeOptions;
        private IIdentityTokenValidator _identityTokenValidator;
        private ILogger _logger;
        private IScopedContext<TenantRequestContext> _scopedTenantRequestContext;

        private static List<string> OneMustExitsArguments => _oneMustExitsArguments ??
                                                             (_oneMustExitsArguments =
                                                                 new List<string>
                                                                 {
                                                                 });

        public TokenExchangeGrantValidator(
            IScopedContext<TenantRequestContext> scopedTenantRequestContext,
            IScopedStorage scopedStorage,
            IResourceStore resourceStore,
            IScopedOptionalClaims scopedOptionalClaims,
            IConsentExternalService consentExternalService,
            IExternalServicesStore externalServicesStore,
            IScopedOverrideRawScopeValues scopedOverrideRawScopeValues,
            ISerializer serializer,
            IConsentDiscoveryCacheAccessor consentDiscoveryCacheAccessor,
            IOptions<TokenExchangeOptions> tokenExchangeOptions,
            IIdentityTokenValidator identityTokenValidator,
            ILogger<TokenExchangeGrantValidator> logger)
        {
            _scopedTenantRequestContext = scopedTenantRequestContext;
            _scopedStorage = scopedStorage;
            _serializer = serializer;
            _resourceStore = resourceStore;
            _scopedOptionalClaims = scopedOptionalClaims;
            _consentExternalService = consentExternalService;
            _externalServicesStore = externalServicesStore;
            _scopedOverrideRawScopeValues = scopedOverrideRawScopeValues;
            _consentDiscoveryCacheAccessor = consentDiscoveryCacheAccessor;
            _tokenExchangeOptions = tokenExchangeOptions.Value;
            _identityTokenValidator = identityTokenValidator;
            _logger = logger;
        }

        public string GrantType => Constants.GrantType.TokenExchange;
        Dictionary<string,List<string>> GetServiceToScopesFromRequest(List<string> requestedScopes)
        {
            var result = new Dictionary<string, List<string>>();
            var index = _tokenExchangeOptions.BaseScope.Length;
            var requestedServiceScopes = (from item in requestedScopes
                                          where item.StartsWith(_tokenExchangeOptions.BaseScope)
                                          select item.Substring(index)).ToList();
            foreach(var item in requestedServiceScopes)
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

        string SubjectFromClaimsPrincipal(ClaimsPrincipal principal)
        {
            if (principal == null) return null;
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
        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var client = context.Request.Client as ClientExtra;
            _scopedTenantRequestContext.Context.Client = client;

            var form = context.Request.Raw;
            var error = false;
            // make sure nothing is malformed
            bool err = false;
            var los = new List<string>();

            // VALIDATE issuer must exist and must be allowed
            // -------------------------------------------------------------------
            var issuer = form.Get("issuer");
            if (string.IsNullOrEmpty(issuer))
            {
                error = true;
                los.Add($"issuer must be present.");
            }
            else
            {
                issuer = issuer.ToLower();
                var foundIssuer = client.AllowedArbitraryIssuers.FirstOrDefault(x => x == issuer);
                if (string.IsNullOrWhiteSpace(foundIssuer))
                {
                    error = true;
                    los.Add($"issuer:{issuer} is NOT in the AllowedArbitraryIssuers collection.");
                }
            }
            _scopedTenantRequestContext.Context.Issuer = issuer;
            error = error || err;
            err = false;

            // MUST have subject
            var subjectToken = form.Get(FluffyBunny4.Constants.TokenExchangeTypes.SubjectToken);
            if (string.IsNullOrWhiteSpace(subjectToken))
            {
                err = true;
                los.Add($"{FluffyBunny4.Constants.TokenExchangeTypes.SubjectToken} is required");
            }
            error = error || err;
            err = false;

            var subjectTokenType = form.Get(FluffyBunny4.Constants.TokenExchangeTypes.SubjectTokenType);
            if (string.IsNullOrWhiteSpace(subjectTokenType))
            {
                err = true;
                los.Add($"{FluffyBunny4.Constants.TokenExchangeTypes.SubjectTokenType} is required");
            }
            error = error || err;
            err = false;

            var subject = "";
            switch (subjectTokenType)
            {

                case FluffyBunny4.Constants.TokenExchangeTypes.IdToken:
                 
                    var validatedResult = await _identityTokenValidator.ValidateIdTokenAsync(subjectToken, _tokenExchangeOptions.AuthorityKey);
                    if (validatedResult.IsError)
                    {
                        err = true;
                        los.Add($"failed to validate id_token");
                        _logger.LogError( $"failed to validate: {FluffyBunny4.Constants.TokenExchangeTypes.SubjectTokenType}={subjectTokenType},{FluffyBunny4.Constants.TokenExchangeTypes.SubjectToken}={subjectToken}\nError={validatedResult.Error}");
                    }

                    subject = SubjectFromClaimsPrincipal(validatedResult.User);
                    if (string.IsNullOrWhiteSpace(subject))
                    {
                        err = true;
                        los.Add($"subject does not exist: {FluffyBunny4.Constants.TokenExchangeTypes.SubjectTokenType}={subjectTokenType},{FluffyBunny4.Constants.TokenExchangeTypes.SubjectToken}={subjectToken}");
                    }
                   

                    break;
                default:
                    err = true;
                    los.Add( $"not supported: {FluffyBunny4.Constants.TokenExchangeTypes.SubjectTokenType}={subjectTokenType},{FluffyBunny4.Constants.TokenExchangeTypes.SubjectToken}={subjectToken}");
                    break;
            }
            error = error || err;
            err = false;

            if (error)
            {
                context.Result.IsError = true;
                context.Result.Error = string.Join<string>(" | ", los);
                _logger.LogError($"context.Result.Error");
                return;
            }

           

            var finalCustomPayload = new Dictionary<string, object>();
         
            var requestedScopesRaw = form[Constants.Scope].Split(' ').ToList();
            var requestedServiceScopes = GetServiceToScopesFromRequest(requestedScopesRaw);
            _scopedOverrideRawScopeValues.IsOverride = true;
            foreach (var serviceScopeSet in requestedServiceScopes)
            {
                var externalService = await _externalServicesStore.GetExternalServiceByNameAsync(serviceScopeSet.Key);
                if (externalService == null)
                {
                    continue;
                }

                var discoCache =
                    await _consentDiscoveryCacheAccessor.GetConsentDiscoveryCacheAsync(serviceScopeSet.Key);
                var doco = await discoCache.GetAsync();
                if (doco.IsError)
                {
                    // OPINION: If I have a lot of external services it it probably better to let this continue even it if 
                    //          results in an access_token that is missing this bad service's scopes.

                    _logger.LogError(doco.Error);
                    continue;
                }

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
                    _scopedOverrideRawScopeValues.Scopes.AddRange(serviceScopeSet.Value);
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
                                _scopedOverrideRawScopeValues.Scopes.AddRange(query);
                                if (response.Claims != null && response.Claims.Any())
                                {
                                    foreach (var cac in response.Claims)
                                    {
                                        // namespace the claims.
                                        _scopedOptionalClaims.Claims.Add(new Claim($"{serviceScopeSet.Key}.{cac.Type}",
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
                _scopedOptionalClaims.Claims.Add(new Claim(
                    Constants.CustomPayload,
                    _serializer.Serialize(finalCustomPayload),
                    IdentityServerConstants.ClaimValueTypes.Json));
            }

            _scopedOptionalClaims.Claims.Add(new Claim(JwtClaimTypes.AuthenticationMethod, GrantType));
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.AuthenticationMethod, GrantType)
            };

            context.Result = new GrantValidationResult(subject, GrantType, claims);
            _scopedStorage.AddOrUpdate(Constants.ScopedRequestType.ExtensionGrantValidationContext, context);
            return;
        }

        private T2 Dictionary<T1, T2>()
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        private void LogError(string message = null, params object[] values)
        {
            if (message.IsPresent())
            {
                try
                {
                    _logger.LogError(message, values);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error logging {exception}", ex.Message);
                }
            }

            //  var details = new global::IdentityServer4.Logging.TokenRequestValidationLog(_validatedRequest);
            //  _logger.LogError("{details}", details);
        }
    }
}
