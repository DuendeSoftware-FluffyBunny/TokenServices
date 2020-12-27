using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.ResponseHandling;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using FluffyBunny4.DotNetCore.Services;
using FluffyBunny4.Extensions;
using FluffyBunny4.Models;
using FluffyBunny4.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FluffyBunny4.Stores
{
    public class MyTokenResponseGenerator: TokenResponseGenerator
    {
        private IScopedStorage _scopedStorage;
        private IPersistedGrantStoreEx _persistedGrantStore;
        private IHttpContextAccessor _contextAccessor;
        private IScopedOptionalClaims _scopedOptionalClaims;
        private IRefreshTokenStore _refreshTokenStore;
        private IReferenceTokenStore _referenceTokenStore;

        public MyTokenResponseGenerator(
            IHttpContextAccessor contextAccessor,
            IScopedOptionalClaims scopedOptionalClaims,
            IScopedStorage scopedStorage,
            IRefreshTokenStore refreshTokenStore,
            IReferenceTokenStore referenceTokenStore,
            IPersistedGrantStoreEx persistedGrantStore,
            ISystemClock clock, 
            ITokenService tokenService, 
            IRefreshTokenService refreshTokenService, 
            IScopeParser scopeParser, 
            IResourceStore resources, 
            IClientStore clients, 
            ILogger<TokenResponseGenerator> logger) : base(clock, tokenService, refreshTokenService, scopeParser, resources, clients, logger)
        {
            _contextAccessor = contextAccessor;
            _scopedOptionalClaims = scopedOptionalClaims;
            _refreshTokenStore = refreshTokenStore;
            _referenceTokenStore = referenceTokenStore;
            _scopedStorage = scopedStorage;
            _persistedGrantStore = persistedGrantStore;
        }

        protected override async Task<TokenResponse> ProcessTokenRequestAsync(TokenRequestValidationResult validationResult)
        {
            switch (validationResult.ValidatedRequest.GrantType)
            {
                case FluffyBunny4.Constants.GrantType.ArbitraryIdentity:

                    var subject =
                                           validationResult.ValidatedRequest.Subject.Claims.FirstOrDefault(x =>
                                               x.Type == JwtClaimTypes.Subject);
                    var resultClaims = new List<Claim>
                    {
                        subject,
                        new Claim(JwtClaimTypes.AuthenticationTime, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    };
                    var accessTokenResultClaims = new List<Claim>(resultClaims);

                    resultClaims.AddRange(_scopedOptionalClaims.Claims);
                    accessTokenResultClaims.AddRange(_scopedOptionalClaims.ArbitraryIdentityAccessTokenClaims);
 
                    var issuer = validationResult.ValidatedRequest.Raw.Get("issuer");
                    if (string.IsNullOrEmpty(issuer))
                    {
                        issuer = _contextAccessor.HttpContext.GetIdentityServerIssuerUri();
                    }

                    var atClaims = accessTokenResultClaims.Distinct(new ClaimComparer()).ToList();
 
                    var at = new Token(OidcConstants.TokenTypes.AccessToken)
                    {
                        CreationTime = Clock.UtcNow.UtcDateTime,
                        Issuer = issuer,
                        Lifetime = validationResult.ValidatedRequest.Client.IdentityTokenLifetime,
                        Claims = atClaims,
                        ClientId = validationResult.ValidatedRequest.ClientId,
                        //    Description = request.Description,
                        AccessTokenType = validationResult.ValidatedRequest.AccessTokenType,
                        AllowedSigningAlgorithms = validationResult.ValidatedRequest.Client.AllowedIdentityTokenSigningAlgorithms,
 
                    };

                    //  var at = await TokenService.CreateAccessTokenAsync(tokenRequest);
                    var accessToken = await TokenService.CreateSecurityTokenAsync(at);


                    var idToken = new Token(OidcConstants.TokenTypes.IdentityToken)
                    {
                        CreationTime = Clock.UtcNow.UtcDateTime,
                      //  Audiences = { aud },
                        Issuer = issuer,
                        Lifetime = validationResult.ValidatedRequest.Client.IdentityTokenLifetime,
                        Claims = resultClaims.Distinct(new ClaimComparer()).ToList(),
                        ClientId = validationResult.ValidatedRequest.ClientId,
                        AccessTokenType = validationResult.ValidatedRequest.AccessTokenType,
                        AllowedSigningAlgorithms = validationResult.ValidatedRequest.Client.AllowedIdentityTokenSigningAlgorithms
                    };
                    var jwtIdToken = await TokenService.CreateSecurityTokenAsync(idToken);
                    var tokenResonse = new TokenResponse
                    {
                        AccessToken = accessToken,
                        IdentityToken = jwtIdToken,
                        AccessTokenLifetime = validationResult.ValidatedRequest.Client.IdentityTokenLifetime,
                    };
                    return tokenResonse;
                    break;
                default:
                   

                    return await base.ProcessTokenRequestAsync(validationResult);
                    break;
            }
        }

        protected async override Task<(string accessToken, string refreshToken)> CreateAccessTokenAsync(ValidatedTokenRequest request)
        {
            TokenCreationRequest tokenRequest;
            bool createRefreshToken;

            if (request.AuthorizationCode != null)
            {
                createRefreshToken = request.AuthorizationCode.RequestedScopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess);

                // load the client that belongs to the authorization code
                Client client = null;
                if (request.AuthorizationCode.ClientId != null)
                {
                    client = await Clients.FindEnabledClientByIdAsync(request.AuthorizationCode.ClientId);
                }
                if (client == null)
                {
                    throw new InvalidOperationException("Client does not exist anymore.");
                }

                var parsedScopesResult = ScopeParser.ParseScopeValues(request.AuthorizationCode.RequestedScopes);
                var validatedResources = await Resources.CreateResourceValidationResult(parsedScopesResult);

                tokenRequest = new TokenCreationRequest
                {
                    Subject = request.AuthorizationCode.Subject,
                    Description = request.AuthorizationCode.Description,
                    ValidatedResources = validatedResources,
                    ValidatedRequest = request
                };
            }
            else if (request.DeviceCode != null)
            {
                createRefreshToken = request.DeviceCode.AuthorizedScopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess);

                Client client = null;
                if (request.DeviceCode.ClientId != null)
                {
                    client = await Clients.FindEnabledClientByIdAsync(request.DeviceCode.ClientId);
                }
                if (client == null)
                {
                    throw new InvalidOperationException("Client does not exist anymore.");
                }

                var parsedScopesResult = ScopeParser.ParseScopeValues(request.DeviceCode.AuthorizedScopes);
                var validatedResources = await Resources.CreateResourceValidationResult(parsedScopesResult);

                tokenRequest = new TokenCreationRequest
                {
                    Subject = request.DeviceCode.Subject,
                    Description = request.DeviceCode.Description,
                    ValidatedResources = validatedResources,
                    ValidatedRequest = request
                };
            }
            else
            {
                createRefreshToken = request.ValidatedResources.Resources.OfflineAccess;

                tokenRequest = new TokenCreationRequest
                {
                    Subject = request.Subject,
                    ValidatedResources = request.ValidatedResources,
                    ValidatedRequest = request
                };
            }


            var at = await TokenService.CreateAccessTokenAsync(tokenRequest);
            object obj;
            if (_scopedStorage.TryGetValue(Constants.ScopedRequestType.OverrideTokenIssuedAtTime, out obj))
            {
                DateTime issuedAtTime = obj is DateTime ? (DateTime) obj : default;
                at.CreationTime = issuedAtTime;
            }
            var accessToken = await TokenService.CreateSecurityTokenAsync(at);

            string refreshToken = null;
            if (createRefreshToken)
            {
                refreshToken = await RefreshTokenService.CreateRefreshTokenAsync(tokenRequest.Subject, at, request.Client);
            }

            if (_scopedStorage.TryGetValue(Constants.ScopedRequestType.ExtensionGrantValidationContext, out obj))
            {
                var extensionGrantValidationContext = obj as ExtensionGrantValidationContext;
                if (extensionGrantValidationContext.Request.GrantType == FluffyBunny4.Constants.GrantType.TokenExchangeMutate)
                {
                    var refreshTokenStoreGrantStoreHashAccessor = _refreshTokenStore as IGrantStoreHashAccessor;
                    var referenceTokenStoreGrantStoreHashAccessor = _referenceTokenStore as IGrantStoreHashAccessor;

                    _scopedStorage.TryGetValue(Constants.ScopedRequestType.PersistedGrantExtra, out obj);
                    var persistedGrantExtra =
                        _scopedStorage.Get<PersistedGrantExtra>(Constants.ScopedRequestType.PersistedGrantExtra);
                    var subjectTokenType =
                        _scopedStorage.Get<string>(Constants.ScopedRequestType.SubjectToken);

                    var newKey = referenceTokenStoreGrantStoreHashAccessor.GetHashedKey(accessToken);
                    var originalKey = referenceTokenStoreGrantStoreHashAccessor.GetHashedKey(subjectTokenType);

                    await _persistedGrantStore.CopyAsync(newKey, originalKey);
                    await _persistedGrantStore.RemoveAsync(newKey);
                    if (!createRefreshToken && !string.IsNullOrWhiteSpace(persistedGrantExtra.RefreshTokenKey))
                    {
                        // need to kill the old refresh token, as this mutate didn't ask for a new one.
                        await _persistedGrantStore.RemoveAsync(persistedGrantExtra.RefreshTokenKey);
                    }
                    else
                    {
                        newKey = refreshTokenStoreGrantStoreHashAccessor.GetHashedKey(refreshToken);
                        await _persistedGrantStore.CopyAsync(newKey, persistedGrantExtra.RefreshTokenKey);
                        await _persistedGrantStore.RemoveAsync(newKey);
                    }

                    // we need to point the old access_token and refresh_token to this new set;
                    return (subjectTokenType, null);  // mutate doesn't get to get the refresh_token back, the original holder of it is the only owner.

                }
            }

            return (accessToken, refreshToken);

     //       return (accessToken, null);
        }
    }
}
