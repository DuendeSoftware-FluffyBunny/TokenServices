using IdentityModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;

namespace FluffyBunny4.Extensions
{
    public static class TokenExtensions
    {
        public static JwtPayload CreateJwtPayload(
            this Token token, 
            DateTime? creationTime, 
            ILogger logger)
        {
            var utcNow = DateTime.UtcNow;
            var iat = creationTime ?? utcNow;
            var payload = new JwtPayload(
                token.Issuer,
                null,
                null,
                utcNow,
                utcNow.AddSeconds(token.Lifetime),
                iat);

            foreach (var aud in token.Audiences)
            {
                payload.AddClaim(new Claim(JwtClaimTypes.Audience, aud));
            }

            var amrClaims = token.Claims.Where(x => x.Type == JwtClaimTypes.AuthenticationMethod);
            var scopeClaims = token.Claims.Where(x => x.Type == JwtClaimTypes.Scope);
            var jsonClaims = token.Claims.Where(x => x.ValueType == IdentityServerConstants.ClaimValueTypes.Json);

            var normalClaims = token.Claims
                .Except(amrClaims)
                .Except(jsonClaims)
                .Except(scopeClaims);

            payload.AddClaims(normalClaims);

            // scope claims
            if (!scopeClaims.IsNullOrEmpty())
            {
                var scopeValues = scopeClaims.Select(x => x.Value).ToArray();
                payload.Add(JwtClaimTypes.Scope, scopeValues);
            }

            // amr claims
            if (!amrClaims.IsNullOrEmpty())
            {
                var amrValues = amrClaims.Select(x => x.Value).Distinct().ToArray();
                payload.Add(JwtClaimTypes.AuthenticationMethod, amrValues);
            }

            // deal with json types
            // calling ToArray() to trigger JSON parsing once and so later 
            // collection identity comparisons work for the anonymous type
            try
            {
                foreach (var jClaim in jsonClaims)
                {
                   // dynamic stuff = JObject.Parse(jClaim.Value);
                    var converter = new ExpandoObjectConverter();
                    dynamic expando = JsonConvert.DeserializeObject<ExpandoObject>(jClaim.Value, converter);
                    payload.Add(jClaim.Type, expando);
                }
 
                return payload;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Error creating a JSON valued claim");
                throw;
            }
        }

        public static JwtSecurityToken CreateJwtSecurityToken(this Token token, DateTime? creationTime, ILogger logger)
        {
            var header = new JwtHeader();
            var payload = token.CreateJwtPayload(creationTime, logger);
            var jwtSecurityToken = new JwtSecurityToken(header, payload);
            return jwtSecurityToken;
        }

    }
}
