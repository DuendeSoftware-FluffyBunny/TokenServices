
using FluffyBunny4.Services;
using FluffyBunny4.Extensions;
using IdentityModel;
using FluffyBunny4.DotNetCore.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluffyBunny4.Models;
using System.Text.RegularExpressions;
using Duende.IdentityServer;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace FluffyBunny4.Validation
{
    public class ArbitraryIdentityGrantValidator : IExtensionGrantValidator
    {
        // private const string _regexExpression = "^[a-zA-Z0-9!@#$%^&*_+=-]*$";

        private const string _regexExpression = @"^[a-zA-Z0-9_\-.:\/]*$";

        private static List<string> NotAllowedIdTokenArbitraryClaims => new List<string>
        {
            "_ns",
            JwtClaimTypes.Subject,
            JwtClaimTypes.Scope,
            JwtClaimTypes.Issuer
        };

        private IHttpContextAccessor _contextAccessor;
        private IResourceStore _resourceStore;
        private IScopedOptionalClaims _scopedOptionalClaims;
        private ILogger _logger;

        private static List<string> OneMustExitsArguments => new List<string>
        {
            "subject"
        };

        public ArbitraryIdentityGrantValidator(
            IHttpContextAccessor contextAccessor,
            IResourceStore resourceStore,
            IScopedOptionalClaims scopedOptionalClaims,
            ILogger<ArbitraryIdentityGrantValidator> logger)
        {
            _contextAccessor = contextAccessor;
            _resourceStore = resourceStore;
            _scopedOptionalClaims = scopedOptionalClaims;
            _logger = logger;
        }

        public string GrantType => Constants.GrantType.ArbitraryIdentity;

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var form = context.Request.Raw;
            var error = false;
            var los = new List<string>();
            
            var oneMustExistResult = (from item in OneMustExitsArguments
                                      where form.AllKeys.Contains(item)
                                      select item).ToList();
            if (!oneMustExistResult.Any())
            {
                error = true;
                los.AddRange(OneMustExitsArguments.Select(item => $"[one or the other] {item} is missing!"));
            }
            var issuer = form.Get("issuer");
            if (string.IsNullOrEmpty(issuer))
            {
                issuer = _contextAccessor.HttpContext.GetIdentityServerIssuerUri();
            }

            // make sure nothing is malformed
            bool err = false;

            Dictionary<string, List<string>> idTokenArbitraryClaims = null;
            (err, idTokenArbitraryClaims) = los.ValidateFormat<Dictionary<string, List<string>>>(Constants.ArbitraryClaims, form[Constants.ArbitraryClaims]);
            error = error || err;
            

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

            if (!error)
            {
                var query = from item in arbitraryJsonClaims
                    let key = item.Key
                        from item2 in idTokenArbitraryClaims
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


            var client = context.Request.Client as ClientExtra;
          
            if (!error)
            {

                if (idTokenArbitraryClaims != null && idTokenArbitraryClaims.Any())
                {

                    var invalidClaims = (from o in idTokenArbitraryClaims
                                         join p in NotAllowedIdTokenArbitraryClaims on o.Key equals p into t
                                         from od in t.DefaultIfEmpty()
                                         where od != null
                                         select od).ToList();
                    if (invalidClaims.Any())
                    {
                        // not allowed.
                        err = true;
                        foreach (var invalidClaim in invalidClaims)
                        {
                            los.Add($"The {Constants.ArbitraryClaims} claim: '{invalidClaim}' is not allowed.");
                        }
                    }
                }
            }
            error = error || err;
            err = false;


            var customPayload = form[Constants.CustomPayload];
            if (!error)
            {
                
                if (!string.IsNullOrWhiteSpace(customPayload))
                {
                    var isValidJson = !customPayload.IsValidJson();
                    if (!isValidJson)
                    {
                        err = true;
                        los.Add($"{Constants.CustomPayload} is not valid json: '{customPayload}'.");
                    }
                }
            }
            error = error || err;
            err = false;


            string subject = "";
            if (!error)
            {
                subject = form.Get("subject");
                if (string.IsNullOrWhiteSpace(subject))
                {
                    err = true;
                    los.Add($"subject must be present.");
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

       
            if (idTokenArbitraryClaims != null)
            {
                foreach (var arbitraryClaimSet in idTokenArbitraryClaims)
                {
                    foreach (var item in arbitraryClaimSet.Value)
                    {
                        _scopedOptionalClaims.Claims.Add(new Claim(arbitraryClaimSet.Key, item));
                    }
                }
            }
            _scopedOptionalClaims.Claims.Add(new Claim(JwtClaimTypes.Audience, issuer));

            var audQuery = (from item in _scopedOptionalClaims.Claims
                           where item.Type == JwtClaimTypes.Audience
                           select item).ToList();
            _scopedOptionalClaims.ArbitraryIdentityAccessTokenClaims.AddRange(audQuery);
            var audScopeQuery = from item in audQuery
                let claim = new Claim(JwtClaimTypes.Scope, item.Value)
                select claim;
            _scopedOptionalClaims.ArbitraryIdentityAccessTokenClaims.AddRange(audScopeQuery);

            if (arbitraryJsonClaims!= null || arbitraryJsonClaims.Any())
            {
                foreach (var arbitraryNode in arbitraryJsonClaims)
                {
                    _scopedOptionalClaims.Claims.Add(new Claim(arbitraryNode.Key, arbitraryNode.Value,
                        IdentityServerConstants.ClaimValueTypes.Json));
                }
               
            }
            
            
            if (!string.IsNullOrWhiteSpace(customPayload))
            {
                _scopedOptionalClaims.Claims.Add(new Claim(Constants.CustomPayload, customPayload,
                    IdentityServerConstants.ClaimValueTypes.Json));
            }

            context.Result = new GrantValidationResult(subject, GrantType, new List<Claim>());
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
