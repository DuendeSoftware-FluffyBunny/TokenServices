using FluffyBunny4.DotNetCore.Extensions;
using FluffyBunny4.Extensions;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.ResponseHandling;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;


namespace FluffyBunny4.Services.Default
{
    public class IntrospectionResponseGenerator : IIntrospectionResponseGenerator
    {
        public IEventService Events { get; }

        private ILogger Logger;

        public IntrospectionResponseGenerator(
            IEventService events,
            ILogger<IntrospectionResponseGenerator> logger)
        {
            Events = events;
            Logger = logger;
        }
        public async Task<Dictionary<string, object>> ProcessAsync(IntrospectionRequestValidationResult validationResult)
        {
            Logger.LogTrace("Creating introspection response");

            // standard response
            var response = new Dictionary<string, object>
            {
                { "active", false }
            };

            // token is invalid
            if (validationResult.IsActive == false)
            {
                Logger.LogDebug("Creating introspection response for inactive token.");
                await Events.RaiseAsync(new TokenIntrospectionSuccessEvent(validationResult));

                return response;
            }
 
            Logger.LogDebug("Creating introspection response for active token.");

            // get all claims (without scopes)
            response = validationResult.Claims.Where(c => c.Type != JwtClaimTypes.Scope).ToClaimsDictionary();

            // add active flag
            response.Add("active", true);

            // calculate scopes the caller is allowed to see
            var scopes = validationResult.Claims.Where(c => c.Type == JwtClaimTypes.Scope).Select(x => x.Value);
            response.Add("scope", scopes.ToSpaceSeparatedString());

            await Events.RaiseAsync(new TokenIntrospectionSuccessEvent(validationResult));
            return response;
        }
    }
}
