using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using FluffyBunny4.Models;
using FluffyBunny4.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace FluffyBunny4.Validation
{
    public class MyDeviceCodeValidator : IDeviceCodeValidator
    {
        private readonly IScopedOptionalClaims _scopedOptionalClaims;
        private readonly IDeviceFlowCodeService _devices;
        private readonly IProfileService _profile;
        private readonly IDeviceFlowThrottlingService _throttlingService;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<MyDeviceCodeValidator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceCodeValidator"/> class.
        /// </summary>
        /// <param name="devices">The devices.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="throttlingService">The throttling service.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public MyDeviceCodeValidator(
            IScopedOptionalClaims scopedOptionalClaims,
            IDeviceFlowCodeService devices,
            IProfileService profile,
            IDeviceFlowThrottlingService throttlingService,
            ISystemClock systemClock,
            ILogger<MyDeviceCodeValidator> logger)
        {
            _scopedOptionalClaims = scopedOptionalClaims;
            _devices = devices;
            _profile = profile;
            _throttlingService = throttlingService;
            _systemClock = systemClock;
            _logger = logger;
        }

        /// <summary>
        /// Validates the device code.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task ValidateAsync(DeviceCodeValidationContext context)
        {
            var deviceCode = await _devices.FindByDeviceCodeAsync(context.DeviceCode);
            var deviceCodeExtra = deviceCode as DeviceCodeExtra;
            if (deviceCode == null)
            {
                _logger.LogError("Invalid device code");
                context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.InvalidGrant);
                return;
            }

            // validate client binding
            if (deviceCode.ClientId != context.Request.Client.ClientId)
            {
                _logger.LogError("Client {0} is trying to use a device code from client {1}", context.Request.Client.ClientId, deviceCode.ClientId);
                context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.InvalidGrant);
                return;
            }

            if (await _throttlingService.ShouldSlowDown(context.DeviceCode, deviceCode))
            {
                _logger.LogError("Client {0} is polling too fast", deviceCode.ClientId);
                context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.SlowDown);
                return;
            }

            // validate lifetime
            if (deviceCode.CreationTime.AddSeconds(deviceCode.Lifetime) < _systemClock.UtcNow)
            {
                _logger.LogError("Expired device code");
                context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.ExpiredToken);
                return;
            }

            // denied
            if (deviceCode.IsAuthorized
                && (deviceCode.AuthorizedScopes == null || deviceCode.AuthorizedScopes.Any() == false))
            {
                _logger.LogError("No scopes authorized for device authorization. Access denied");
                context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.AccessDenied);
                return;
            }

            // make sure code is authorized
            if (!deviceCode.IsAuthorized || deviceCode.Subject == null)
            {
                context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.AuthorizationPending);
                return;
            }

            // make sure user is enabled
            var isActiveCtx = new IsActiveContext(deviceCode.Subject, context.Request.Client, IdentityServerConstants.ProfileIsActiveCallers.DeviceCodeValidation);
            await _profile.IsActiveAsync(isActiveCtx);

            if (isActiveCtx.IsActive == false)
            {
                _logger.LogError("User has been disabled: {subjectId}", deviceCode.Subject.GetSubjectId());
                context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.InvalidGrant);
                return;
            }

            context.Request.DeviceCode = deviceCode;
            context.Request.SessionId = deviceCode.SessionId;
            if (deviceCodeExtra.AuthorizedClaims != null || deviceCodeExtra.AuthorizedClaims.Any())
            {
                _scopedOptionalClaims.Claims.AddRange(deviceCodeExtra.AuthorizedClaims);
            }
            context.Result = new TokenRequestValidationResult(context.Request);
            await _devices.RemoveByDeviceCodeAsync(context.DeviceCode);
        }
    }
}
