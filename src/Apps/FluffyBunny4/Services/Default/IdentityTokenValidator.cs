using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.OidcClient.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace FluffyBunny4.Services.Default
{
    internal class IdentityTokenValidator : IIdentityTokenValidator
    {
        private IDiscoveryCacheAccessor _discoveryCacheAccessor;
        private ILogger<IdentityTokenValidator> _logger;

        

        public IdentityTokenValidator(IDiscoveryCacheAccessor discoveryCacheAccessor,ILogger<IdentityTokenValidator> logger)
        {
            _discoveryCacheAccessor = discoveryCacheAccessor;
            _logger = logger;
        }
        public async Task<IdentityTokenValidationResult> ValidateIdTokenAsync(string identityToken, string issuerKey)
        {
            try
            {
                var discoCache = _discoveryCacheAccessor.GetCache(issuerKey);
                var doco = await discoCache.GetAsync();
                var handler = new JwtSecurityTokenHandler();
                handler.InboundClaimTypeMap.Clear();

                var parameters = new TokenValidationParameters
                {
                    ValidIssuer = doco.Issuer,
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    NameClaimType = JwtClaimTypes.Name,
                    RoleClaimType = JwtClaimTypes.Role,
                    ClockSkew = new TimeSpan(0, 5, 0)
                };
                // read the token signing algorithm
                JwtSecurityToken jwt;

                try
                {
                    jwt = handler.ReadJwtToken(identityToken);
                }
                catch (Exception ex)
                {
                    return new IdentityTokenValidationResult
                    {
                        Error = $"Error validating identity token: {ex.ToString()}"
                    };
                }
                var algorithm = jwt.Header.Alg;
                // if token is unsigned, and this is allowed, skip signature validation
                if (string.IsNullOrWhiteSpace(algorithm) || string.Equals(algorithm, "none"))
                {
                    return new IdentityTokenValidationResult
                    {
                        Error = $"Identity token is not singed. Signatures are required by policy"
                    };
                }
                ClaimsPrincipal user;
                user = ValidateSignature(identityToken, handler, parameters, doco.KeySet);
                return new IdentityTokenValidationResult
                {
                    User = user,
                    SignatureAlgorithm = algorithm
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new IdentityTokenValidationResult
                {
                    Error = ex.Message,
                    User = null,
                    SignatureAlgorithm = null
                };
            }
          
           
        }

        private ClaimsPrincipal ValidateSignature(
            string identityToken,
            JwtSecurityTokenHandler handler,
            TokenValidationParameters parameters,
            IdentityModel.Jwk.JsonWebKeySet keySet)
        {
            if (parameters.RequireSignedTokens)
            {
                // read keys from provider information
                var keys = new List<SecurityKey>();

                foreach (var webKey in keySet.Keys)
                {
                    if (webKey.E.IsPresent() && webKey.N.IsPresent())
                    {
                        // only add keys used for signatures
                        if (webKey.Use == "sig" || webKey.Use == null)
                        {
                            var e = Base64Url.Decode(webKey.E);
                            var n = Base64Url.Decode(webKey.N);

                            var key = new RsaSecurityKey(new RSAParameters {Exponent = e, Modulus = n});
                            key.KeyId = webKey.Kid;

                            keys.Add(key);

                            _logger.LogDebug("Added signing key with kid: {kid}", key?.KeyId ?? "not set");
                        }
                    }
                    else if (webKey.X.IsPresent() && webKey.Y.IsPresent() && webKey.Crv.IsPresent())
                    {

                        var ec = ECDsa.Create(new ECParameters
                        {
                            Curve = GetCurveFromCrvValue(webKey.Crv),
                            Q = new ECPoint
                            {
                                X = Base64Url.Decode(webKey.X),
                                Y = Base64Url.Decode(webKey.Y)
                            }
                        });

                        var key = new ECDsaSecurityKey(ec);
                        key.KeyId = webKey.Kid;

                        keys.Add(key);

                    }
                    else
                    {
                        _logger.LogDebug("Signing key with kid: {kid} currently not supported",
                            webKey.Kid ?? "not set");
                    }
                }

                parameters.IssuerSigningKeys = keys;
            }

            SecurityToken token;
            return handler.ValidateToken(identityToken, parameters, out token);
        }

        internal static ECCurve GetCurveFromCrvValue(string crv)
        {
            switch (crv)
            {
                case JsonWebKeyECTypes.P256:
                    return ECCurve.NamedCurves.nistP256;
                case JsonWebKeyECTypes.P384:
                    return ECCurve.NamedCurves.nistP384;
                case JsonWebKeyECTypes.P521:
                    return ECCurve.NamedCurves.nistP521;
                default:
                    throw new InvalidOperationException($"Unsupported curve type of {crv}");
            }
        }
    }
}