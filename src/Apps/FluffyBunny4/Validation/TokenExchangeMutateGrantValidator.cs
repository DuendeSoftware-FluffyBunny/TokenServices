
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
    public class TokenExchangeMutateGrantValidator : IExtensionGrantValidator
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
        private ITokenValidator _tokenValidator;
        private ILogger _logger;
        private IPersistedGrantStore _persistedGrantStore;
        private IReferenceTokenStore _referenceTokenStore;

        private static List<string> OneMustExitsArguments => _oneMustExitsArguments ??
                                                                  (_oneMustExitsArguments =
                                                                      new List<string>
                                                                      {
                                                                      });

        public TokenExchangeMutateGrantValidator(
            IReferenceTokenStore referenceTokenStore,
            IPersistedGrantStore persistedGrantStore,
            IScopedStorage scopedStorage,
            IResourceStore resourceStore,
            IScopedOptionalClaims scopedOptionalClaims,
            IConsentExternalService consentExternalService,
            IExternalServicesStore externalServicesStore,
            IScopedOverrideRawScopeValues scopedOverrideRawScopeValues,
            ISerializer serializer,
            IConsentDiscoveryCacheAccessor consentDiscoveryCacheAccessor,
            IOptions<TokenExchangeOptions> tokenExchangeOptions,
            ITokenValidator tokenValidator,
            ILogger<TokenExchangeMutateGrantValidator> logger)
        {
            _persistedGrantStore = persistedGrantStore;
            _referenceTokenStore = referenceTokenStore;
            _scopedStorage = scopedStorage;
            _serializer = serializer;
            _resourceStore = resourceStore;
            _scopedOptionalClaims = scopedOptionalClaims;
            _consentExternalService = consentExternalService;
            _externalServicesStore = externalServicesStore;
            _scopedOverrideRawScopeValues = scopedOverrideRawScopeValues;
            _consentDiscoveryCacheAccessor = consentDiscoveryCacheAccessor;
            _tokenExchangeOptions = tokenExchangeOptions.Value;
            _tokenValidator = tokenValidator;
            _logger = logger;
        }

        public string GrantType => Constants.GrantType.TokenExchangeMutate;
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
            //TODO FANOUT


            var externalServices = await _externalServicesStore.GetExternalServicesAsync();
            var form = context.Request.Raw;
            var error = false;
            var los = new List<string>();
            
            // make sure nothing is malformed
            bool err = false;

            // MUST have subject
            var subjectToken = form.Get(FluffyBunny4.Constants.TokenExchangeTypes.SubjectToken);
            if (string.IsNullOrWhiteSpace(subjectToken))
            {
                context.Result.IsError = true;
                los.Add($"{FluffyBunny4.Constants.TokenExchangeTypes.SubjectToken} is required");
                context.Result.Error = string.Join<string>(" | ", los);
                return;
            }
            var subjectTokenType = form.Get(FluffyBunny4.Constants.TokenExchangeTypes.SubjectTokenType);
            if (string.IsNullOrWhiteSpace(subjectTokenType))
            {
                context.Result.IsError = true;
                los.Add($"{FluffyBunny4.Constants.TokenExchangeTypes.SubjectTokenType} is required");
                context.Result.Error = string.Join<string>(" | ", los);
                return;
            }

            DateTime tokenIssuedAtTime;
            List<Claim> claims;
            var subject = "";
            switch (subjectTokenType)
            {
                case FluffyBunny4.Constants.TokenExchangeTypes.AccessToken:
                    if (subjectToken.Contains('.'))
                    {
                        // only refrerence tokens accepted.
                        throw new Exception(
                            $"failed to validate, not a reference_token: {FluffyBunny4.Constants.TokenExchangeTypes.SubjectTokenType}={subjectTokenType},{FluffyBunny4.Constants.TokenExchangeTypes.AccessToken}={subjectToken}");
                    }
                    var validatedResultAccessToken = await _tokenValidator.ValidateAccessTokenAsync(subjectToken);
                    if (validatedResultAccessToken.IsError)
                    {
                        throw new Exception(
                            $"failed to validate: {FluffyBunny4.Constants.TokenExchangeTypes.SubjectTokenType}={subjectTokenType},{FluffyBunny4.Constants.TokenExchangeTypes.AccessToken}={subjectToken}",
                            new Exception(validatedResultAccessToken.Error));
                    }

                    subject = validatedResultAccessToken.Claims
                        .FirstOrDefault(claim => claim.Type == JwtClaimTypes.Subject).Value;

                    claims = validatedResultAccessToken.Claims.ToList();
                    var amr = claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.AuthenticationMethod &&
                                                             claim.Value == Constants.GrantType.TokenExchange);

                    if (amr == null)
                    {
                        throw new Exception(
                            $"failed to validate, missing amr={Constants.GrantType.TokenExchange}: {FluffyBunny4.Constants.TokenExchangeTypes.SubjectTokenType}={subjectTokenType},{FluffyBunny4.Constants.TokenExchangeTypes.AccessToken}={subjectToken}",
                            new Exception(validatedResultAccessToken.Error));
                    }

                    var issuedAt = claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.IssuedAt);
                    if (issuedAt == null)
                    {
                        throw new Exception(
                            $"failed to validate, {JwtClaimTypes.IssuedAt} is missing: {FluffyBunny4.Constants.TokenExchangeTypes.SubjectTokenType}={subjectTokenType},{FluffyBunny4.Constants.TokenExchangeTypes.AccessToken}={subjectToken}",
                            new Exception(validatedResultAccessToken.Error));
                    }

                    var unixSeconds = Convert.ToInt64(issuedAt.Value);
                    DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);

                    tokenIssuedAtTime = dateTimeOffset.UtcDateTime;

                    var referenceTokenStoreGrantStoreHashAccessor = _referenceTokenStore as IGrantStoreHashAccessor;
                    var hashKey = referenceTokenStoreGrantStoreHashAccessor.GetHashedKey(subjectToken);
                    var accessTokenPersitedGrant = await _persistedGrantStore.GetAsync(hashKey);
                    if (accessTokenPersitedGrant == null)
                    {
                        throw new Exception(
                            $"failed to validate, accessTokenPersitedGrant is missing: {FluffyBunny4.Constants.TokenExchangeTypes.SubjectTokenType}={subjectTokenType},{FluffyBunny4.Constants.TokenExchangeTypes.AccessToken}={subjectToken}");
                    }
                    _scopedStorage.AddOrUpdate(Constants.ScopedRequestType.SubjectToken, subjectToken);
                    _scopedStorage.AddOrUpdate(Constants.ScopedRequestType.PersistedGrantExtra, accessTokenPersitedGrant as PersistedGrantExtra);
                    break;

                
                default:
                    throw new Exception($"not supported: {FluffyBunny4.Constants.TokenExchangeTypes.SubjectTokenType}={subjectTokenType},{FluffyBunny4.Constants.TokenExchangeTypes.SubjectToken}={subjectToken}");
                    break;
            }

            var client = context.Request.Client as ClientExtra;

            var finalCustomPayload = new Dictionary<string, object>();
         
            var requestedScopesRaw = form[Constants.Scope].Split(' ').ToList();
            var requestedServiceScopes = GetServiceToScopesFromRequest(requestedScopesRaw);
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
            claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.AuthenticationMethod, GrantType)
            };

            context.Result = new GrantValidationResult(subject, GrantType, tokenIssuedAtTime,claims);
            _scopedStorage.AddOrUpdate(Constants.ScopedRequestType.ExtensionGrantValidationContext, context);
            _scopedStorage.AddOrUpdate(Constants.ScopedRequestType.OverrideTokenIssuedAtTime, tokenIssuedAtTime);
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
