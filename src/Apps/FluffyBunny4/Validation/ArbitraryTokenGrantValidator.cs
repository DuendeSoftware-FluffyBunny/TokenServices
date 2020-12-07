
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

namespace FluffyBunny4.Validation
{
    public class ArbitraryTokenGrantValidator : IExtensionGrantValidator
    {
        // private const string _regexExpression = "^[a-zA-Z0-9!@#$%^&*_+=-]*$";

        private const string _regexExpression = @"^[a-zA-Z0-9_\-.:\/]*$";

        private static List<string> _notAllowedArbitraryClaims;
        private static List<string> NotAllowedArbitraryClaims => _notAllowedArbitraryClaims ??
                                                                 (_notAllowedArbitraryClaims =
                                                                     new List<string>
                                                                     {
                                                                         "_ns",
                                                                         ClaimTypes.NameIdentifier,
                                                                         ClaimTypes.AuthenticationMethod,
                                                                         JwtClaimTypes.AccessTokenHash,
                                                                         JwtClaimTypes.Audience,
                                                                         JwtClaimTypes.AuthenticationMethod,
                                                                         JwtClaimTypes.AuthenticationTime,
                                                                         JwtClaimTypes.AuthorizedParty,
                                                                         JwtClaimTypes.AuthorizationCodeHash,
                                                                         JwtClaimTypes.ClientId,
                                                                         JwtClaimTypes.Expiration,
                                                                         JwtClaimTypes.IdentityProvider,
                                                                         JwtClaimTypes.IssuedAt,
                                                                         JwtClaimTypes.Issuer,
                                                                         JwtClaimTypes.JwtId,
                                                                         JwtClaimTypes.Nonce,
                                                                         JwtClaimTypes.NotBefore,
                                                                         JwtClaimTypes.ReferenceTokenId,
                                                                         JwtClaimTypes.SessionId,
                                                                         JwtClaimTypes.Subject,
                                                                         JwtClaimTypes.Scope,
                                                                         JwtClaimTypes.Confirmation,
                                                                         "custom_payload"
                                                                     });

        private static List<string> _oneMustExitsArguments;
        private IResourceStore _resourceStore;
        private IScopedOptionalClaims _scopedOptionalClaims;
        private ILogger _logger;

        private static List<string> OneMustExitsArguments => _oneMustExitsArguments ??
                                                                  (_oneMustExitsArguments =
                                                                      new List<string>
                                                                      {
                                                                      });

        public ArbitraryTokenGrantValidator(
            IResourceStore resourceStore,
            IScopedOptionalClaims scopedOptionalClaims,
            ILogger<ArbitraryTokenGrantValidator> logger)
        {
            _resourceStore = resourceStore;
            _scopedOptionalClaims = scopedOptionalClaims;
            _logger = logger;
        }

        public string GrantType => Constants.GrantType.ArbitraryToken;

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var form = context.Request.Raw;
            var error = false;
            var los = new List<string>();
            /*
            var oneMustExistResult = (from item in OneMustExitsArguments
                                      where form.AllKeys.Contains(item)
                                      select item).ToList();
            if (!oneMustExistResult.Any())
            {
                error = true;
                los.AddRange(OneMustExitsArguments.Select(item => $"[one or the other] {item} is missing!"));
            }
            */
            // make sure nothing is malformed
            bool err = false;

            Dictionary<string, List<string>> arbitraryClaims = null;
            (err, arbitraryClaims) = los.ValidateFormat<Dictionary<string, List<string>>>(Constants.ArbitraryClaims, form[Constants.ArbitraryClaims]);
            error = error || err;

            List<string> arbitraryAmrs = null;
            (err, arbitraryAmrs) = los.ValidateFormat<List<string>>(Constants.ArbitraryAmrs, form[Constants.ArbitraryAmrs]);
            error = error || err;

            List<string> arbitraryAudiences;
            (err, arbitraryAudiences) = los.ValidateFormat<List<string>>(Constants.ArbitraryAudiences, form[Constants.ArbitraryAudiences]);
            error = error || err;

            List<string> requestedScopes = new List<string>();

            var resources = await _resourceStore.GetAllEnabledResourcesAsync();
            var apiResources = resources.ApiResources.ToList();
            var wellKnownScopes = apiResources.ToScopeNames();

            var client = context.Request.Client as ClientExtra;

            var allowedScopes = wellKnownScopes.Intersect(client.AllowedScopes).ToList();
            var notAllowedScope = wellKnownScopes.Where(item => !allowedScopes.Contains(item)).ToList();

            List<string> arbitraryScopes = new List<string>();
            var arbitaryScopesRaw = form[Constants.Scope];
            if (!string.IsNullOrWhiteSpace(arbitaryScopesRaw))
            {
                arbitraryScopes.AddRange(arbitaryScopesRaw.Split(' '));
                arbitraryScopes = arbitraryScopes.Where(item => !notAllowedScope.Contains(item)).ToList();
                
                Regex r = new Regex(_regexExpression);
                err = false;
                foreach (var scope in arbitraryScopes)
                {
                    if (!r.IsMatch(scope))
                    {
                        err = true;
                        los.Add($"scope:'{scope}' is invalid, must match the following regex expression:'{_regexExpression}'.");
                    }
                }
                error = error || err;
            }
            err = false;
            if (!error)
            {

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
            }
            if (!error)
            {
                var customPayloadRaw = form[Constants.CustomPayload];
                if (!string.IsNullOrWhiteSpace(customPayloadRaw))
                {
                    error = !customPayloadRaw.IsValidJson();
                    if (error)
                    {
                        los.Add($"{Constants.CustomPayload} is not valid: '{customPayloadRaw}'.");
                    }
                }
            }
            if (error)
            {
                context.Result.IsError = true;
                context.Result.Error = string.Join<string>(" | ", los);
                return;
            }
            var subject = form.Get("subject");
            if (string.IsNullOrEmpty(subject))
            {
                subject = Constants.ReservedSubject;
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

            if (arbitraryAmrs != null)
            {
                foreach (var item in arbitraryAmrs)
                {
                    claims.Add(new Claim(JwtClaimTypes.AuthenticationMethod, item));
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
            if (arbitraryAudiences != null)
            {
                foreach (var item in arbitraryAudiences)
                {
                    _scopedOptionalClaims.Claims.Add(new Claim(JwtClaimTypes.Audience, item));
                }
            }

            var customPayload = form[Constants.CustomPayload];
            if (!string.IsNullOrWhiteSpace(customPayload))
            {
                _scopedOptionalClaims.Claims.Add(new Claim(Constants.CustomPayload, customPayload,
                    IdentityServerConstants.ClaimValueTypes.Json));
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
