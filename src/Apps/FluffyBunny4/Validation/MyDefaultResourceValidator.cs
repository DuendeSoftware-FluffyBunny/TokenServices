using FluffyBunny4;
using FluffyBunny4.Services;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Default implementation of IResourceValidator.
    /// </summary>
    public class MyDefaultResourceValidator : DefaultResourceValidator
    {
        private readonly IScopedHttpContextRequestForm _scopedHttpContextRequestForm;
        private readonly ILogger _logger;

        public MyDefaultResourceValidator(
            IResourceStore store,
            IScopeParser scopeParser,
            IScopedHttpContextRequestForm scopedHttpContextRequestForm,
            ILogger<DefaultResourceValidator> logger) : base(store, scopeParser, logger)
        {
            _scopedHttpContextRequestForm = scopedHttpContextRequestForm;
            _logger = logger;
        }

        /// <summary>
        /// Validates that the requested scopes is contained in the store, and the client is allowed to request it.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="resourcesFromStore"></param>
        /// <param name="requestedScope"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected override async Task ValidateScopeAsync(Client client, Resources resourcesFromStore, 
            ParsedScopeValue requestedScope, ResourceValidationResult result)
        {
            var parameters = await _scopedHttpContextRequestForm.GetFormCollectionAsync();
            var grantType = parameters.Get(OidcConstants.TokenRequest.GrantType);
            switch (grantType)
            {
                case Constants.GrantType.TokenExchangeMutate:
                case Constants.GrantType.TokenExchange:
                case Constants.GrantType.ArbitraryToken:
                    if (requestedScope.ParsedName == IdentityServerConstants.StandardScopes.OfflineAccess)
                    {
                        if (await IsClientAllowedOfflineAccessAsync(client))
                        {
                            result.Resources.OfflineAccess = true;
                            result.ParsedScopes.Add(new ParsedScopeValue(IdentityServerConstants.StandardScopes.OfflineAccess));
                        }
                        else
                        {
                            result.InvalidScopes.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
                        }
                    }
                    else
                    {
                        result.ParsedScopes.Add(requestedScope);
                    }
                    break;
                default:
                    await base.ValidateScopeAsync(client, resourcesFromStore, requestedScope, result);
                    break;
            }
        }

        /// <summary>
        /// Determines if client is allowed access to the API scope.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="apiScope"></param>
        /// <returns></returns>
        protected override async Task<bool> IsClientAllowedApiScopeAsync(Client client, ApiScope apiScope)
        {
            var parameters = await _scopedHttpContextRequestForm.GetFormCollectionAsync();
            var grantType = parameters.Get(OidcConstants.TokenRequest.GrantType);
            if (grantType == Constants.GrantType.ArbitraryToken)
            {
                return true;
            }
            return await base.IsClientAllowedApiScopeAsync(client, apiScope);
        }
    }
}

