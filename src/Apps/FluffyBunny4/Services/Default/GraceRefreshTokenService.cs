
using FluffyBunny4.DotNetCore.Extensions;
using FluffyBunny4.Extensions;
using FluffyBunny4.Models;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using FluffyBunny4.DotNetCore.Services;

namespace FluffyBunny4.Services.Default
{

    public class GraceRefreshTokenService : DefaultRefreshTokenService
    {
        private IScopedHttpContextRequestForm _scopedHttpContextRequestForm;

        public GraceRefreshTokenService(
            IScopedHttpContextRequestForm scopedHttpContextRequestForm,
            IRefreshTokenStore refreshTokenStore,
            IProfileService profile,
            ISystemClock clock,
            ILogger<DefaultRefreshTokenService> logger) :
            base(refreshTokenStore, profile, clock, logger)
        {
            _scopedHttpContextRequestForm = scopedHttpContextRequestForm;
        }

        public async Task<string> CreateRefreshTokenAsync(
            ClaimsPrincipal subject, Token accessToken, Client client)
        {
            Logger.LogDebug("Creating refresh token");

            int lifetime;
            if (client.RefreshTokenExpiration == TokenExpiration.Absolute)
            {
                Logger.LogDebug("Setting an absolute lifetime: {absoluteLifetime}",
                    client.AbsoluteRefreshTokenLifetime);
                lifetime = client.AbsoluteRefreshTokenLifetime;
            }
            else
            {
                lifetime = client.SlidingRefreshTokenLifetime;
                if (client.AbsoluteRefreshTokenLifetime > 0 && lifetime > client.AbsoluteRefreshTokenLifetime)
                {
                    Logger.LogWarning(
                        "Client {clientId}'s configured " + nameof(client.SlidingRefreshTokenLifetime) +
                        " of {slidingLifetime} exceeds its " + nameof(client.AbsoluteRefreshTokenLifetime) +
                        " of {absoluteLifetime}. The refresh_token's sliding lifetime will be capped to the absolute lifetime",
                        client.ClientId, lifetime, client.AbsoluteRefreshTokenLifetime);
                    lifetime = client.AbsoluteRefreshTokenLifetime;
                }

                Logger.LogDebug("Setting a sliding lifetime: {slidingLifetime}", lifetime);
            }

            var formCollection = await _scopedHttpContextRequestForm.GetFormCollectionAsync();
            var originGrantType = formCollection["grant_type"];
            var refreshToken = new RefreshTokenExtra
            {
                CreationTime = Clock.UtcNow.UtcDateTime,
                Lifetime = lifetime,
                AccessToken = accessToken,
                OriginGrantType = originGrantType
            };

            var handle = await RefreshTokenStore.StoreRefreshTokenAsync(refreshToken);
            return handle;
        }

        public override async Task<string> CreateRefreshTokenAsync(RefreshTokenCreationRequest request)
        {
            Logger.LogDebug("Creating refresh token");

            int lifetime;
            if (request.Client.RefreshTokenExpiration == TokenExpiration.Absolute)
            {
                Logger.LogDebug("Setting an absolute lifetime: {absoluteLifetime}",
                    request.Client.AbsoluteRefreshTokenLifetime);
                lifetime = request.Client.AbsoluteRefreshTokenLifetime;
            }
            else
            {
                lifetime = request.Client.SlidingRefreshTokenLifetime;
                if (request.Client.AbsoluteRefreshTokenLifetime > 0 && lifetime > request.Client.AbsoluteRefreshTokenLifetime)
                {
                    Logger.LogWarning(
                        "Client {clientId}'s configured " + nameof(request.Client.SlidingRefreshTokenLifetime) +
                        " of {slidingLifetime} exceeds its " + nameof(request.Client.AbsoluteRefreshTokenLifetime) +
                        " of {absoluteLifetime}. The refresh_token's sliding lifetime will be capped to the absolute lifetime",
                        request.Client.ClientId, lifetime, request.Client.AbsoluteRefreshTokenLifetime);
                    lifetime = request.Client.AbsoluteRefreshTokenLifetime;
                }

                Logger.LogDebug("Setting a sliding lifetime: {slidingLifetime}", lifetime);
            }
            var formCollection = await _scopedHttpContextRequestForm.GetFormCollectionAsync();
            var originGrantType = formCollection["grant_type"];

            var refreshToken = new RefreshTokenExtra()
            {
                Subject = request.Subject,
                ClientId = request.Client.ClientId,
                Description = request.Description,
                AuthorizedScopes = request.AuthorizedScopes,
                AuthorizedResourceIndicators = request.AuthorizedResourceIndicators,

                CreationTime = Clock.UtcNow.UtcDateTime,
                Lifetime = lifetime,
                OriginGrantType = originGrantType
            };
            refreshToken.SetAccessToken(request.AccessToken, request.RequestedResourceIndicator);

            var handle = await RefreshTokenStore.StoreRefreshTokenAsync(refreshToken);
            return handle;
        }

        public override async Task<string> UpdateRefreshTokenAsync(RefreshTokenUpdateRequest request)
        {
            Logger.LogDebug("Updating refresh token");

            var refreshTokenExtra = request.RefreshToken as RefreshTokenExtra;
            var client = request.Client;
            var clientExtra = client as ClientExtra;

            var handle = request.Handle.Substring(2);
            bool needsCreate = false;
            bool needsUpdate = request.MustUpdate;

            if (request.Client.RefreshTokenUsage == TokenUsage.OneTimeOnly)
            {
                Logger.LogDebug("Token usage is one-time only. Setting current handle as consumed, and generating new handle");

                // flag as consumed
                if (request.RefreshToken.ConsumedTime == null)
                {
                    request.RefreshToken.ConsumedTime = Clock.UtcNow.UtcDateTime;
                }

                // increment the attempts used
                refreshTokenExtra.ConsumedAttempts += 1;
                if (!clientExtra.RefreshTokenGraceEnabled)
                {
                    await RefreshTokenStore.UpdateRefreshTokenAsync(handle, refreshTokenExtra);
                }

                // create new one
                needsCreate = true;
            }

            if (request.Client.RefreshTokenExpiration == TokenExpiration.Sliding)
            {
                Logger.LogDebug("Refresh token expiration is sliding - extending lifetime");

                // if absolute exp > 0, make sure we don't exceed absolute exp
                // if absolute exp = 0, allow indefinite slide
                var currentLifetime = request.RefreshToken.CreationTime.GetLifetimeInSeconds(Clock.UtcNow.UtcDateTime);
                Logger.LogDebug("Current lifetime: {currentLifetime}", currentLifetime.ToString());

                var newLifetime = currentLifetime + request.Client.SlidingRefreshTokenLifetime;
                Logger.LogDebug("New lifetime: {slidingLifetime}", newLifetime.ToString());

                // zero absolute refresh token lifetime represents unbounded absolute lifetime
                // if absolute lifetime > 0, cap at absolute lifetime
                if (request.Client.AbsoluteRefreshTokenLifetime > 0 && newLifetime > request.Client.AbsoluteRefreshTokenLifetime)
                {
                    newLifetime = request.Client.AbsoluteRefreshTokenLifetime;
                    Logger.LogDebug("New lifetime exceeds absolute lifetime, capping it to {newLifetime}",
                        newLifetime.ToString());
                }

                request.RefreshToken.Lifetime = newLifetime;
                needsUpdate = true;
            }

            if (needsCreate)
            {
                var oldChild = refreshTokenExtra.RefeshTokenChild;
                var oldParent = refreshTokenExtra.RefeshTokenParent;
                var savedConsumedTime = refreshTokenExtra.ConsumedTime;
                var savedConsumedAttempts = refreshTokenExtra.ConsumedAttempts;

                // set it to null so that we save non-consumed token
                refreshTokenExtra.ConsumedTime = null;
                refreshTokenExtra.ConsumedAttempts = 0;
                refreshTokenExtra.RefeshTokenChild = null;
                // carry forward the parent.
                refreshTokenExtra.RefeshTokenParent = handle;

                var newHandle = await RefreshTokenStore.StoreRefreshTokenAsync(refreshTokenExtra);

                refreshTokenExtra.ConsumedTime = savedConsumedTime;
                refreshTokenExtra.ConsumedAttempts = savedConsumedAttempts;
                refreshTokenExtra.RefeshTokenParent = null;
                if (client.RefreshTokenUsage == TokenUsage.OneTimeOnly)
                {
                    Logger.LogDebug("Token usage is one-time only. Setting current handle as consumed, and generating new handle");
                    if (clientExtra.RefreshTokenGraceEnabled)
                    {
                        refreshTokenExtra.RefeshTokenChild = newHandle;
                    }

                    await RefreshTokenStore.UpdateRefreshTokenAsync(
                        handle,
                        refreshTokenExtra);

                    if (clientExtra.RefreshTokenGraceEnabled)
                    {
                        if (!string.IsNullOrWhiteSpace(oldChild))
                        {
                            await RefreshTokenStore.RemoveRefreshTokenAsync(oldChild);
                        }
                        if (!string.IsNullOrWhiteSpace(oldParent))
                        {
                            await RefreshTokenStore.RemoveRefreshTokenAsync(oldParent);
                        }
                    }
                }
                handle = newHandle;
                Logger.LogDebug("Created refresh token in store");
            }
            else if (needsUpdate)
            {
                await RefreshTokenStore.UpdateRefreshTokenAsync(handle, request.RefreshToken);
                Logger.LogDebug("Updated refresh token in store");
            }
            else
            {
                Logger.LogDebug("No updates to refresh token done");
            }

            return handle;
        }

        public  async Task<string> UpdateRefreshTokenAsync(
            string handle,
            RefreshToken refreshToken,
            Client client)
        {
            Logger.LogDebug("Updating refresh token");

            var refreshTokenExtra = refreshToken as RefreshTokenExtra;
            var clientExtra = client as ClientExtra;

            bool needsCreate = false;
            bool needsUpdate = false;

            if (client.RefreshTokenUsage == TokenUsage.OneTimeOnly)
            {
                Logger.LogDebug("Token usage is one-time only. Setting current handle as consumed, and generating new handle");

                // flag as consumed
                if (!refreshTokenExtra.ConsumedTime.HasValue)
                {
                    // only track the initial consumed time.
                    refreshTokenExtra.ConsumedTime = Clock.UtcNow.DateTime;
                }
                // increment the attempts used
                refreshTokenExtra.ConsumedAttempts += 1;
                if (!clientExtra.RefreshTokenGraceEnabled)
                {
                    await RefreshTokenStore.UpdateRefreshTokenAsync(handle, refreshTokenExtra);
                }

                // create new one
                needsCreate = true;
            }

            if (client.RefreshTokenExpiration == TokenExpiration.Sliding)
            {
                Logger.LogDebug("Refresh token expiration is sliding - extending lifetime");

                // if absolute exp > 0, make sure we don't exceed absolute exp
                // if absolute exp = 0, allow indefinite slide
                var currentLifetime = refreshTokenExtra.CreationTime.GetLifetimeInSeconds(Clock.UtcNow.UtcDateTime);
                Logger.LogDebug("Current lifetime: {currentLifetime}", currentLifetime.ToString());

                var newLifetime = currentLifetime + client.SlidingRefreshTokenLifetime;
                Logger.LogDebug("New lifetime: {slidingLifetime}", newLifetime.ToString());

                // zero absolute refresh token lifetime represents unbounded absolute lifetime
                // if absolute lifetime > 0, cap at absolute lifetime
                if (client.AbsoluteRefreshTokenLifetime > 0 && newLifetime > client.AbsoluteRefreshTokenLifetime)
                {
                    newLifetime = client.AbsoluteRefreshTokenLifetime;
                    Logger.LogDebug("New lifetime exceeds absolute lifetime, capping it to {newLifetime}",
                        newLifetime.ToString());
                }

                refreshTokenExtra.Lifetime = newLifetime;
                needsUpdate = true;
            }

            if (needsCreate)
            {
                var oldChild = refreshTokenExtra.RefeshTokenChild;
                var oldParent = refreshTokenExtra.RefeshTokenParent;
                var savedConsumedTime = refreshTokenExtra.ConsumedTime;

                // set it to null so that we save non-consumed token
                refreshTokenExtra.ConsumedTime = null;
                refreshTokenExtra.RefeshTokenChild = null;
                // carry forward the parent.
                refreshTokenExtra.RefeshTokenParent = handle;

                var newHandle = await RefreshTokenStore.StoreRefreshTokenAsync(refreshTokenExtra);

                refreshTokenExtra.ConsumedTime = savedConsumedTime;
                refreshTokenExtra.RefeshTokenParent = null;
                if (client.RefreshTokenUsage == TokenUsage.OneTimeOnly)
                {
                    Logger.LogDebug("Token usage is one-time only. Setting current handle as consumed, and generating new handle");
                    if (clientExtra.RefreshTokenGraceEnabled)
                    {
                        refreshTokenExtra.RefeshTokenChild = newHandle;
                    }

                    await RefreshTokenStore.UpdateRefreshTokenAsync(
                           handle,
                           refreshTokenExtra);

                    if (clientExtra.RefreshTokenGraceEnabled)
                    {
                        if (!string.IsNullOrWhiteSpace(oldChild))
                        {
                            await RefreshTokenStore.RemoveRefreshTokenAsync(oldChild);
                        }
                        if (!string.IsNullOrWhiteSpace(oldParent))
                        {
                            await RefreshTokenStore.RemoveRefreshTokenAsync(oldParent);
                        }
                    }
                }
                handle = newHandle;
                Logger.LogDebug("Created refresh token in store");
            }
            else if (needsUpdate)
            {
                await RefreshTokenStore.UpdateRefreshTokenAsync(handle, refreshTokenExtra);
                Logger.LogDebug("Updated refresh token in store");
            }
            else
            {
                Logger.LogDebug("No updates to refresh token done");
            }

            return handle;
        }

        public override async Task<TokenValidationResult> ValidateRefreshTokenAsync(
            string tokenHandle, Client client)
        {
            //////////////////////////////////////////////////
            // Calling the base ValidateRefreshTokenAsync
            // this covers the obvious failures like Lifetime checks.
            //////////////////////////////////////////////////

            var baseResult = await base.ValidateRefreshTokenAsync(tokenHandle, client);
            if (baseResult.IsError)
            {
               // await RefreshTokenStore.RemoveRefreshTokenAsync(tokenHandle);
                return baseResult;
            }
            var invalidGrant = new TokenValidationResult
            {
                IsError = true,
                Error = OidcConstants.TokenErrors.InvalidGrant
            };
            /////////////////////////////////////////////
            // check if refresh token has been consumed
            /////////////////////////////////////////////
            if (baseResult.RefreshToken.ConsumedTime.HasValue)
            {
                if (await AcceptConsumedTokenAsync(baseResult.RefreshToken, client) == false)
                {
                    Logger.LogWarning("Rejecting refresh token because it has been consumed already.");
                    await RefreshTokenStore.RemoveRefreshTokenAsync(tokenHandle);
                    return invalidGrant;
                }
            }
            baseResult.Client = client;
            return baseResult;

        }

        protected override Task<bool> AcceptConsumedTokenAsync(RefreshToken refreshToken)
        {
            // we will reject this later in the flow
            return Task.FromResult(true);
        }

        protected virtual Task<bool> AcceptConsumedTokenAsync(RefreshToken refreshToken, Client client)
        {
            var refreshTokenExtra = refreshToken as RefreshTokenExtra;
            var clientExtra = client as ClientExtra;

            if (!clientExtra.RefreshTokenGraceEnabled)
            {
                if (client.RefreshTokenUsage == TokenUsage.OneTimeOnly && refreshToken.ConsumedTime.HasValue)
                {
                    Logger.LogWarning("OneTimeOnly token has already been consumed.");
                    return Task.FromResult(false);
                }
                return Task.FromResult(true);
            }
            else
            {
                /////////////////////////////////////////////
                // check if grace refresh token has expired
                /////////////////////////////////////////////
                var consumedTime = (DateTime)refreshTokenExtra.ConsumedTime;
                if (consumedTime.HasExceeded(
                      clientExtra.RefreshTokenGraceTTL
                    , Clock.UtcNow.DateTime))
                {
                    Logger.LogWarning("Refresh token has expired.");
                    return Task.FromResult(false);
                }
                if (refreshTokenExtra.ConsumedAttempts >= clientExtra.RefreshTokenGraceMaxAttempts)
                {
                    Logger.LogWarning("Refresh token exceeded RefreshTokenGraceMaxAttempts.");
                    return Task.FromResult(false);
                }

                return Task.FromResult(true);
            }
        }
    }
}
