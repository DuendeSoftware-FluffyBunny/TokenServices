using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using IdentityModel;
using IdentityServer4.Validation;
using FluffyBunny4.Models;
using System.Reflection.Metadata;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;

namespace FluffyBunny4.Services
{
    public class MyDefaultClaimsService : DefaultClaimsService
    {
        private IScopedOptionalClaims _scopedOptionalClaims;
        private IScopedOverrideRawScopeValues _scopedOverrideRawScopeValues;

        public MyDefaultClaimsService(
            IProfileService profile,
            IScopedOptionalClaims scopedOptionalClaims,
            IScopedOverrideRawScopeValues scopedOverrideRawScopeValues,
            ILogger<MyDefaultClaimsService> logger) : base(profile, logger)
        {
            _scopedOptionalClaims = scopedOptionalClaims;
            _scopedOverrideRawScopeValues = scopedOverrideRawScopeValues;
        }
        public override async Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal subject,
            ResourceValidationResult resourceResult, ValidatedRequest request)
        {
            if (_scopedOverrideRawScopeValues.Scopes.Any())
            {
                var scopes = new HashSet<ParsedScopeValue>();
                foreach (var s in _scopedOverrideRawScopeValues.Scopes)
                {
                    scopes.Add((new ParsedScopeValue(s)));
                }

                resourceResult.ParsedScopes = scopes;
            }
            var claims = await base.GetAccessTokenClaimsAsync(subject, resourceResult, request);
            var clientExtra = request.Client as ClientExtra;
            if (!clientExtra.IncludeClientId)
            {
                claims = from claim in claims
                            where claim.Type != JwtClaimTypes.ClientId
                            select claim;
            }
            if (!clientExtra.IncludeAmr)
            {
                var queryAmr = from claim in claims
                               where claim.Type == JwtClaimTypes.AuthenticationMethod
                               select claim;
                if (queryAmr.Count() == 1)
                {
                    // only meant to remove the single one that IDS4 added.
                    claims = from claim in claims
                             where claim.Type != JwtClaimTypes.AuthenticationMethod
                            select claim;
                }
            }
            var subjectClaim = (from claim in claims
                            where claim.Type == JwtClaimTypes.Subject
                            select claim).FirstOrDefault();
            if(subjectClaim != null && subjectClaim.Value == Constants.ReservedSubject)
            {
                claims = from claim in claims
                         where claim.Type != JwtClaimTypes.Subject
                         select claim;
            }
            return claims;
        }

        protected override IEnumerable<Claim> GetOptionalClaims(ClaimsPrincipal subject)
        {
            var claims = new List<Claim>();
            claims.AddRange(base.GetOptionalClaims(subject));
            if (_scopedOptionalClaims.Claims != null)
            {
                claims.AddRange(_scopedOptionalClaims.Claims);
            }
            return claims;
        }
    }
}

