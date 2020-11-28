using FluffyBunny4.Models;
using FluffyBunny4.Services;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;

namespace FluffyBunny4.Validation
{
    public class MyClientSecretValidator : IClientSecretValidator
    {
        private readonly ILogger _logger;
        private readonly IClientStore _clients;
        private readonly IEventService _events;
        private readonly ITenantRequestContext _tenantRequestContext;
        private readonly ISecretsListValidator _validator;
        private readonly ISecretsListParser _parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSecretValidator"/> class.
        /// </summary>
        /// <param name="clients">The clients.</param>
        /// <param name="parser">The parser.</param>
        /// <param name="validator">The validator.</param>
        /// <param name="events">The events.</param>
        /// <param name="logger">The logger.</param>
        public MyClientSecretValidator(
            IClientStore clients, 
            ISecretsListParser parser, 
            ISecretsListValidator validator, 
            IEventService events,
            ITenantRequestContext tenantRequestContext,
            ILogger<ClientSecretValidator> logger)
        {
            _clients = clients;
            _parser = parser;
            _validator = validator;
            _events = events;
            _tenantRequestContext = tenantRequestContext;
            _logger = logger;
        }

        /// <summary>
        /// Validates the current request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<ClientSecretValidationResult> ValidateAsync(HttpContext context)
        {
            _logger.LogDebug("Start client validation");

            var fail = new ClientSecretValidationResult
            {
                IsError = true
            };

            var parsedSecret = await _parser.ParseAsync(context);
            if (parsedSecret == null)
            {
                await RaiseFailureEventAsync("unknown", "No client id found");

                _logger.LogError("No client identifier found");
                return fail;
            }

            // load client
            var client = await _clients.FindEnabledClientByIdAsync(parsedSecret.Id) as ClientExtra;
            if (client == null)
            {
                await RaiseFailureEventAsync(parsedSecret.Id, $"Unknown client for tenant: '{_tenantRequestContext.TenantId}'");

                _logger.LogError($"No client with id '{parsedSecret.Id}' for tenant: '{_tenantRequestContext.TenantId}' found. aborting");
                return fail;
            }
            

            SecretValidationResult secretValidationResult = null;
            if (!client.RequireClientSecret || client.IsImplicitOnly())
            {
                _logger.LogDebug("Public Client - skipping secret validation success");
            }
            else
            {
                ////////////////////////////////////////
                // Check if this is a refresh_token
                ////////////////////////////////////////
                bool continueValidation = true;
                if (!client.RequireRefreshClientSecret)
                {
                    var parameters = (await context.Request.ReadFormAsync()).AsNameValueCollection();
                    var grantType = parameters.Get(OidcConstants.TokenRequest.GrantType);
                    if (!string.IsNullOrWhiteSpace(grantType) && grantType == OidcConstants.GrantTypes.RefreshToken)
                    {
                        // let it through
                        _logger.LogDebug("RequireRefreshClientSecret == false - skipping secret validation success");
                        continueValidation = false;
                    }
                }
                if (continueValidation)
                {
                    secretValidationResult = await _validator.ValidateAsync(client.ClientSecrets, parsedSecret);
                    if (secretValidationResult.Success == false)
                    {
                        await RaiseFailureEventAsync(client.ClientId, "Invalid client secret");
                        _logger.LogError("Client secret validation failed for client: {clientId}.", client.ClientId);

                        return fail;
                    }
                }
            }

            _logger.LogDebug("Client validation success");

            var success = new ClientSecretValidationResult
            {
                IsError = false,
                Client = client,
                Secret = parsedSecret,
                Confirmation = secretValidationResult?.Confirmation
            };

            await RaiseSuccessEventAsync(client.ClientId, parsedSecret.Type);
            return success;
        }

        private Task RaiseSuccessEventAsync(string clientId, string authMethod)
        {
            return _events.RaiseAsync(new ClientAuthenticationSuccessEvent(clientId, authMethod));
        }

        private Task RaiseFailureEventAsync(string clientId, string message)
        {
            return _events.RaiseAsync(new ClientAuthenticationFailureEvent(clientId, message));
        }
    }
}

