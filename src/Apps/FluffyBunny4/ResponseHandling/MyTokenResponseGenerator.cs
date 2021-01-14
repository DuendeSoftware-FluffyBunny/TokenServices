using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
using FluffyBunny4.Stores;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FluffyBunny4.ResponseHandling
{
    public class MyTokenResponseGenerator: TokenResponseGenerator
    {
        private IScopedStorage _scopedStorage;
        private IPersistedGrantStoreEx _persistedGrantStore;
        private IScopedHttpContextRequestForm _scopedHttpContextRequestForm;
        private IHttpContextAccessor _contextAccessor;
        private IScopedOptionalClaims _scopedOptionalClaims;
        private IRefreshTokenStore _refreshTokenStore;
        private IReferenceTokenStore _referenceTokenStore;

        public MyTokenResponseGenerator(
            IScopedHttpContextRequestForm scopedHttpContextRequestForm,
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
            _scopedHttpContextRequestForm = scopedHttpContextRequestForm;
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
                case FluffyBunny4.Constants.GrantType.ArbitraryToken:
                    return await ProcessArbitraryTokenTokenResponse(validationResult);
                    break;
                case FluffyBunny4.Constants.GrantType.ArbitraryIdentity:
                    return await ProcessArbitraryIdentityTokenResponse(validationResult);
                    break;
                default:
                    return await base.ProcessTokenRequestAsync(validationResult);
                    break;
            }
        }

        protected override async Task<TokenResponse> ProcessRefreshTokenRequestAsync(TokenRequestValidationResult request)
        {
            var response = await base.ProcessRefreshTokenRequestAsync(request);
            var refreshTokenExtra = request.ValidatedRequest.RefreshToken as RefreshTokenExtra;

            switch (refreshTokenExtra.OriginGrantType)
            {
                case FluffyBunny4.Constants.GrantType.ArbitraryIdentity:
                case FluffyBunny4.Constants.GrantType.ArbitraryToken:
                case OidcConstants.GrantTypes.DeviceCode:
                case FluffyBunny4.Constants.GrantType.TokenExchange:
                case FluffyBunny4.Constants.GrantType.TokenExchangeMutate:
                    response.RefreshToken = $"1_{response.RefreshToken}";
                    if (!string.IsNullOrWhiteSpace(response.AccessToken) && !response.AccessToken.Contains("."))
                    {
                        response.AccessToken = $"1_{response.AccessToken}";
                    }

                    break;
                default:
                    response.RefreshToken = $"0_{response.RefreshToken}";
                    if (!string.IsNullOrWhiteSpace(response.AccessToken) && !response.AccessToken.Contains("."))
                    {
                        response.AccessToken = $"0_{response.AccessToken}";
                    }

                    break;
            }

            return response;
        }

        private async Task<TokenResponse> ProcessArbitraryIdentityTokenResponse(TokenRequestValidationResult validationResult)
        {
            var form = validationResult.ValidatedRequest.Raw;
            var subject =
                validationResult.ValidatedRequest.Subject.Claims.FirstOrDefault(x =>
                    x.Type == JwtClaimTypes.Subject);
            var resultClaims = new List<Claim>
            {
                subject,
                new Claim(JwtClaimTypes.AuthenticationTime, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
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

            var tokenLifetimeOverride = form.Get(Constants.IdTokenLifetime);
            int tokenLifetime = 0;
            int.TryParse(tokenLifetimeOverride, out tokenLifetime);

            var at = new Token(OidcConstants.TokenTypes.AccessToken)
            {
                CreationTime = Clock.UtcNow.UtcDateTime,
                Issuer = issuer,
                Lifetime = tokenLifetime,
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
                Lifetime = tokenLifetime,
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
                AccessTokenLifetime = tokenLifetime
            };
            return tokenResonse;
        }

        private async Task<TokenResponse> ProcessArbitraryTokenTokenResponse(TokenRequestValidationResult validationResult)
        {
            var subject =
                validationResult.ValidatedRequest.Subject.Claims.FirstOrDefault(x =>
                    x.Type == JwtClaimTypes.Subject);


            var form = validationResult.ValidatedRequest.Raw;
            var resultClaims = new List<Claim>()
            {
                subject
            };
            resultClaims.AddRange(_scopedOptionalClaims.Claims);
            var issuer = validationResult.ValidatedRequest.Raw.Get("issuer");
            if (string.IsNullOrEmpty(issuer))
            {
                issuer = _contextAccessor.HttpContext.GetIdentityServerIssuerUri();
            }

            var atClaims = resultClaims.Distinct(new ClaimComparer()).ToList();

            bool createRefreshToken;

            var offlineAccessClaim =
                atClaims.FirstOrDefault(x =>
                    x.Type == JwtClaimTypes.Scope && x.Value == IdentityServerConstants.StandardScopes.OfflineAccess);
            createRefreshToken = offlineAccessClaim != null;

            var accessTokenLifetimeOverride = form.Get(Constants.AccessTokenLifetime);
            int accessTokenLifetime = 0;
            int.TryParse(accessTokenLifetimeOverride, out accessTokenLifetime);

            var at = new Token(OidcConstants.TokenTypes.AccessToken)
            {
                CreationTime = Clock.UtcNow.UtcDateTime,
                Issuer = issuer,
                Lifetime = accessTokenLifetime,
                Claims = atClaims,
                ClientId = validationResult.ValidatedRequest.ClientId,
                //    Description = request.Description,
                AccessTokenType = validationResult.ValidatedRequest.AccessTokenType,
                AllowedSigningAlgorithms = validationResult.ValidatedRequest.Client.AllowedIdentityTokenSigningAlgorithms,
            };
            var accessToken = await TokenService.CreateSecurityTokenAsync(at);

            string refreshToken = null;
            if (createRefreshToken)
            {
                var refreshTokenCreationRequest = new RefreshTokenCreationRequest
                {
                    Subject = validationResult.ValidatedRequest.Subject,
                    AccessToken = at,
                    Client = validationResult.ValidatedRequest.Client
                };
                refreshToken = await RefreshTokenService.CreateRefreshTokenAsync(refreshTokenCreationRequest);
            }

            var tokenResonse = new TokenResponse
            {
                AccessToken = accessToken,
                IdentityToken = null,
                RefreshToken = refreshToken,
                AccessTokenLifetime = accessTokenLifetime
            };
            return tokenResonse;
        }

        protected async override Task<(string accessToken, string refreshToken)> CreateAccessTokenAsync(ValidatedTokenRequest request)
        {
            var tokenRequest = new TokenCreationRequest
            {
                Subject = request.Subject,
                ValidatedResources = request.ValidatedResources,
                ValidatedRequest = request
            };

            bool createRefreshToken;
            var authorizedScopes = Enumerable.Empty<string>();
            IEnumerable<string> authorizedResourceIndicators = null;

            if (request.AuthorizationCode != null)
            {
                createRefreshToken = request.ValidatedResources.Resources.OfflineAccess;

                //                createRefreshToken = request.AuthorizationCode.RequestedScopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess);

                // load the client that belongs to the authorization code
                Client client = null;
                if (request.AuthorizationCode.ClientId != null)
                {
                    // todo: do we need this check?
                    client = await Clients.FindEnabledClientByIdAsync(request.AuthorizationCode.ClientId);
                }
                if (client == null)
                {
                    throw new InvalidOperationException("Client does not exist anymore.");
                }

                tokenRequest.Subject = request.AuthorizationCode.Subject;
                tokenRequest.Description = request.AuthorizationCode.Description;

                authorizedScopes = request.AuthorizationCode.RequestedScopes;
                authorizedResourceIndicators = request.AuthorizationCode.RequestedResourceIndicators;
            }
            else if (request.DeviceCode != null)
            {
                createRefreshToken = request.DeviceCode.AuthorizedScopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess);

                Client client = null;
                if (request.DeviceCode.ClientId != null)
                {
                    // todo: do we need this check?
                    client = await Clients.FindEnabledClientByIdAsync(request.DeviceCode.ClientId);
                }
                if (client == null)
                {
                    throw new InvalidOperationException("Client does not exist anymore.");
                }

                tokenRequest.Subject = request.DeviceCode.Subject;
                tokenRequest.Description = request.DeviceCode.Description;

                authorizedScopes = request.DeviceCode.AuthorizedScopes;
            }
            else
            {
                createRefreshToken = request.RequestedScopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess);
                authorizedScopes = request.ValidatedResources.RawScopeValues;
            }


            var at = await TokenService.CreateAccessTokenAsync(tokenRequest);
            object obj;
            if (_scopedStorage.TryGetValue(Constants.ScopedRequestType.OverrideTokenIssuedAtTime, out obj))
            {
                DateTime issuedAtTime = obj is DateTime ? (DateTime) obj : default;
                at.CreationTime = issuedAtTime;
            }

            var finalScopes = at.Scopes.ToList();
            if (createRefreshToken)
            {
                if (!finalScopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess))
                {
                    finalScopes.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
                }
            }
            else
            {
                if (finalScopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess))
                {
                    finalScopes.Remove(IdentityServerConstants.StandardScopes.OfflineAccess);
                }
            }
         
            var accessToken = await TokenService.CreateSecurityTokenAsync(at);

            string refreshToken = null;
            if (createRefreshToken)
            {
                var rtRequest = new RefreshTokenCreationRequest
                {
                    Client = request.Client,
                    Subject = tokenRequest.Subject,
                    Description = tokenRequest.Description,
                    AuthorizedScopes = authorizedScopes,
                    AuthorizedResourceIndicators = authorizedResourceIndicators,
                    AccessToken = at,
                    RequestedResourceIndicator = request.RequestedResourceIndicator,
                };

                
                refreshToken = await RefreshTokenService.CreateRefreshTokenAsync(rtRequest);
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
            var formCollection = _scopedHttpContextRequestForm.GetFormCollection();
            var grantType = formCollection["grant_type"];

            switch (grantType)
            {
                case FluffyBunny4.Constants.GrantType.ArbitraryIdentity:
                case FluffyBunny4.Constants.GrantType.ArbitraryToken:
                case OidcConstants.GrantTypes.DeviceCode:
                case FluffyBunny4.Constants.GrantType.TokenExchange:
                case FluffyBunny4.Constants.GrantType.TokenExchangeMutate:
                    if (!accessToken.Contains('.'))
                    {
                        accessToken = $"1_{accessToken}";
                    }
                   
                    if (!string.IsNullOrWhiteSpace(refreshToken))
                    {
                        refreshToken = $"1_{refreshToken}";
                    }

                    break;
                default:
                    if (!accessToken.Contains('.'))
                    {
                        accessToken = $"0_{accessToken}";
                    }
                    if (!string.IsNullOrWhiteSpace(refreshToken))
                    {
                        refreshToken = $"0_{refreshToken}";
                    }

                    break;
            }

            return (accessToken, refreshToken);

            //       return (accessToken, null);
        }
    }
}
