using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using FluffyBunny4.DotNetCore.Services;
using FluffyBunny4.Extensions;
using FluffyBunny4.Models;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FluffyBunny4.Services.Default
{
    public class MyDefaultTokenService : DefaultTokenService
    {
        private IScopedContext<TenantRequestContext> _scopedTenantRequestContext;

        public MyDefaultTokenService(
            IScopedContext<TenantRequestContext> scopedTenantRequestContext,
            IClaimsService claimsProvider,
            IReferenceTokenStore referenceTokenStore,
            ITokenCreationService creationService,
            IHttpContextAccessor contextAccessor,
            ISystemClock clock,
            IKeyMaterialService keyMaterialService,
            IdentityServerOptions options,
            ILogger<DefaultTokenService> logger) : base(claimsProvider, referenceTokenStore, creationService,
            contextAccessor, clock, keyMaterialService, options, logger)
        {
            _scopedTenantRequestContext = scopedTenantRequestContext;
        }

        public override async Task<Token> CreateAccessTokenAsync(TokenCreationRequest request)
        {
            Logger.LogTrace("Creating access token");
            request.Validate();

            var claims = new List<Claim>();
            claims.AddRange(await ClaimsProvider.GetAccessTokenClaimsAsync(
                request.Subject,
                request.ValidatedResources,
                request.ValidatedRequest));

            if (request.ValidatedRequest.Client.IncludeJwtId)
            {
                claims.Add(new Claim(JwtClaimTypes.JwtId, CryptoRandom.CreateUniqueId(16, CryptoRandom.OutputFormat.Hex)));
            }

            if (request.ValidatedRequest.SessionId.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.SessionId, request.ValidatedRequest.SessionId));
            }

            var issuer = _scopedTenantRequestContext.Context.Issuer;

            if (string.IsNullOrWhiteSpace(issuer))
            {
                issuer = ContextAccessor.HttpContext.GetIdentityServerIssuerUri();
            }

            var token = new Token(OidcConstants.TokenTypes.AccessToken)
            {
                CreationTime = Clock.UtcNow.UtcDateTime,
                Issuer = issuer,
                Lifetime = request.ValidatedRequest.AccessTokenLifetime,
                Claims = claims.Distinct(new ClaimComparer()).ToList(),
                ClientId = request.ValidatedRequest.Client.ClientId,
                Description = request.Description,
                AccessTokenType = request.ValidatedRequest.AccessTokenType,
                AllowedSigningAlgorithms = request.ValidatedResources.Resources.ApiResources.FindMatchingSigningAlgorithms()
            };

            // add aud based on ApiResources in the validated request
            foreach (var aud in request.ValidatedResources.Resources.ApiResources.Select(x => x.Name).Distinct())
            {
                token.Audiences.Add(aud);
            }

            if (Options.EmitStaticAudienceClaim)
            {
                token.Audiences.Add(string.Format(IdentityServerConstants.AccessTokenAudience, issuer.EnsureTrailingSlash()));
            }

            // add cnf if present
            if (request.ValidatedRequest.Confirmation.IsPresent())
            {
                token.Confirmation = request.ValidatedRequest.Confirmation;
            }
            else
            {
                if (Options.MutualTls.AlwaysEmitConfirmationClaim)
                {
                    var clientCertificate = await ContextAccessor.HttpContext.Connection.GetClientCertificateAsync();
                    if (clientCertificate != null)
                    {
                        token.Confirmation = clientCertificate.CreateThumbprintCnf();
                    }
                }
            }

            return token;
        }
    }
}
