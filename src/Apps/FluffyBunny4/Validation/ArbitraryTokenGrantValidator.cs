
using FluffyBunny4.Services;
using FluffyBunny4.Extensions;
using IdentityModel;
using FluffyBunny4.DotNetCore.Extensions;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace FluffyBunny4.Validation
{
    public class ArbitraryTokenGrantValidator : IExtensionGrantValidator
    {
        // private const string _regexExpression = "^[a-zA-Z0-9!@#$%^&*_+=-]*$";

        private const string _regexExpression = @"^[a-zA-Z0-9_\-.:\/]*$";

        private static List<string> _notAllowedArbitraryClaims;
        private static List<string> NotAllowedArbitraryClaims =>  new List<string>
                                                                     {
                                                                         JwtClaimTypes.Issuer,
                                                                         JwtClaimTypes.Subject
                                                                     };

      
        private IScopedOptionalClaims _scopedOptionalClaims;
        private ILogger _logger;

        public ArbitraryTokenGrantValidator(
            IScopedOptionalClaims scopedOptionalClaims,
            ILogger<ArbitraryTokenGrantValidator> logger)
        {
            _scopedOptionalClaims = scopedOptionalClaims;
            _logger = logger;
        }

        public string GrantType => Constants.GrantType.ArbitraryToken;

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var client = context.Request.Client as ClientExtra;
            
            // make sure nothing is malformed
            bool err = false;
            bool error = false;

            var form = context.Request.Raw;
          
            var los = new List<string>();

            // VALIDATE subject must exist  
            // -------------------------------------------------------------------
            var subject = form.Get("subject");
            if (string.IsNullOrEmpty(subject))
            {
                err = true;
                los.Add($"subject must be present.");
            }
            error = error || err;
            err = false;

            // VALIDATE issuer must exist and must be allowed
            // -------------------------------------------------------------------
            var issuer = form.Get("issuer");
            if (string.IsNullOrEmpty(issuer))
            {
                err = true;
                los.Add($"issuer must be present.");
            }
            else
            {
                issuer = issuer.ToLower();
                var foundIssuer = client.AllowedArbitraryIssuers.FirstOrDefault(x => x == issuer);
                if (string.IsNullOrWhiteSpace(foundIssuer))
                {
                    err = true;
                    los.Add($"issuer:{issuer} is NOT in the AllowedArbitraryIssuers collection.");
                }
            }
            error = error || err;
            err = false;

            // VALIDATE arbitrary_claims is in the correct format
            // -------------------------------------------------------------------
            Dictionary<string, List<string>> arbitraryClaims = null;
            (err, arbitraryClaims) = los.ValidateFormat<Dictionary<string, List<string>>>(Constants.ArbitraryClaims, form[Constants.ArbitraryClaims]);
            error = error || err;
            err = false;

            // VALIDATE arbitrary_json is in the correct format
            // -------------------------------------------------------------------
            var arbitraryJsonClaims = new Dictionary<string, string>();
            var arbitraryJson = form[Constants.ArbitraryJson];
            if (!string.IsNullOrWhiteSpace(arbitraryJson))
            {
                try
                {
                    JObject jsonObj = JObject.Parse(arbitraryJson);
                    var dictObjs = jsonObj.ToObject<Dictionary<string, object>>();

                    var converter = new ExpandoObjectConverter();

                    foreach (var dictObj in dictObjs)
                    {
                        var value = JsonConvert.SerializeObject(dictObj.Value);
                        dynamic stuff = JObject.Parse(value);  // will throw if not an object
                        arbitraryJsonClaims.Add(dictObj.Key, value);
                    }
                }
                catch (Exception ex)
                {
                    err = true;
                    los.Add($"The {Constants.ArbitraryJson}: is malformed, toplevel arrays not allowed.");
                }

            }
            error = error || err;
            err = false;

            // VALIDATE arbitrary_json and arbitrary_claims don't intersect
            // -------------------------------------------------------------------
            if (!error)
            {
                var query = from item in arbitraryJsonClaims
                    let key = item.Key
                    from item2 in arbitraryClaims
                            where item2.Key == key
                    select key;
                if (query.Any())
                {
                    // collision
                    err = true;
                    foreach (var qItem in query)
                    {
                        los.Add($"The {qItem}: must either be unique in {Constants.ArbitraryClaims} or {Constants.ArbitraryJson}.");
                    }
                }
            }
            error = error || err;
            err = false;

            // VALIDATE arbitrary_claims doesn't have any disallowed claims
            // -------------------------------------------------------------------
            if (arbitraryClaims != null && arbitraryClaims.Any())
            {
                var invalidClaims = (from o in arbitraryClaims
                                     join p in NotAllowedArbitraryClaims on o.Key equals p into t
                                     from od in t.DefaultIfEmpty()
                                     where od != null
                                     select od).ToList();
                if (invalidClaims.Any())
                {
                    // not allowed.
                    error = true;
                    foreach (var invalidClaim in invalidClaims)
                    {
                        los.Add($"The arbitrary claim: '{invalidClaim}' is not allowed.");
                    }
                }
            }
            error = error || err;
            err = false;

            // VALIDATE arbitrary_json doesn't have any disallowed claims
            // -------------------------------------------------------------------
            if (arbitraryJsonClaims != null && arbitraryJsonClaims.Any())
            {
                var invalidClaims = (from o in arbitraryJsonClaims
                                     join p in NotAllowedArbitraryClaims on o.Key equals p into t
                    from od in t.DefaultIfEmpty()
                    where od != null
                    select od).ToList();
                if (invalidClaims.Any())
                {
                    // not allowed.
                    error = true;
                    foreach (var invalidClaim in invalidClaims)
                    {
                        los.Add($"The arbitrary json claim: '{invalidClaim}' is not allowed.");
                    }
                }
            }
            error = error || err;
            err = false;

           
            if (error)
            {
                context.Result.IsError = true;
                context.Result.Error = string.Join<string>(" | ", los);
                return;
            }
           
            var claims = new List<Claim>();


            var accessTokenTypeOverride = form.Get(Constants.AccessTokenType);
            if (!string.IsNullOrWhiteSpace(accessTokenTypeOverride))
            {
                error = true;
                if (string.Compare(accessTokenTypeOverride, "Reference", true) == 0 ||
                    string.Compare(accessTokenTypeOverride, "Jwt", true) == 0)
                {
                    error = false;
                }
                if (error)
                {
                    var errorDescription =
                        $"{Constants.AccessTokenType} out of range.   Must be reference or jwt.";
                    LogError(errorDescription);
                    context.Result.IsError = true;
                    context.Result.Error = errorDescription;
                    context.Result.ErrorDescription = errorDescription;
                    return;
                }
            }
            
            // optional stuff;
            var accessTokenLifetimeOverride = form.Get(Constants.AccessTokenLifetime);
            if (!string.IsNullOrWhiteSpace(accessTokenLifetimeOverride))
            {
                int accessTokenLifetime = 0;
                error = true;
                if (int.TryParse(accessTokenLifetimeOverride, out accessTokenLifetime))
                {
                    if (accessTokenLifetime > 0 && accessTokenLifetime <= client.AccessTokenLifetime)
                    {
                        // HERB: Setting this sets it for the global config.
                        //client.AccessTokenLifetime = accessTokenLifetime;
                        error = false;
                    }
                }
                if (error)
                {
                    var errorDescription =
                        $"{Constants.AccessTokenLifetime} out of range.   Must be > 0 and <= configured AccessTokenLifetime.";
                    LogError(errorDescription);
                    context.Result.IsError = true;
                    context.Result.Error = errorDescription;
                    context.Result.ErrorDescription = errorDescription;
                    return;
                }
            }

             
            if (arbitraryClaims != null)
            {
                foreach (var arbitraryClaimSet in arbitraryClaims)
                {
                    foreach (var item in arbitraryClaimSet.Value)
                    {
                        _scopedOptionalClaims.Claims.Add(new Claim(arbitraryClaimSet.Key, item));
                    }
                }
            }
            if (arbitraryJsonClaims != null || arbitraryJsonClaims.Any())
            {
                foreach (var arbitraryNode in arbitraryJsonClaims)
                {
                    _scopedOptionalClaims.Claims.Add(new Claim(arbitraryNode.Key, arbitraryNode.Value,
                        IdentityServerConstants.ClaimValueTypes.Json));
                }

            }

            context.Result = new GrantValidationResult(subject, GrantType, claims);
            return;
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
