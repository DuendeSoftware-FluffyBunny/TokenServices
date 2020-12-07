using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;

namespace FluffyBunny4.Validation
{
    public class FluffyBunnyCustomTokenRequestValidator : ICustomTokenRequestValidator
    {
        public Task ValidateAsync(CustomTokenRequestValidationContext context)
        {
            // overriding access_token_lifetime
            var form = context.Result.ValidatedRequest.Raw;
            var accessTokenLifetimeOverride = form.Get(Constants.AccessTokenLifetime);
            if (!string.IsNullOrWhiteSpace(accessTokenLifetimeOverride))
            {
                // already been validated in a previous phase
                int accessTokenLifetime = 0;
                if (int.TryParse(accessTokenLifetimeOverride, out accessTokenLifetime))
                {
                    context.Result.ValidatedRequest.AccessTokenLifetime = accessTokenLifetime;
                }
            }
            var accessTokenTypeOverride = form.Get(Constants.AccessTokenType);
            if (!string.IsNullOrWhiteSpace(accessTokenTypeOverride))
            {
               

                if (string.Compare(accessTokenTypeOverride, "Reference", true) == 0 )
                {
                    context.Result.ValidatedRequest.AccessTokenType = AccessTokenType.Reference;
                }
                else if (string.Compare(accessTokenTypeOverride, "Jwt", true) == 0)
                {
                    context.Result.ValidatedRequest.AccessTokenType = AccessTokenType.Jwt;
                }
            }
            return Task.CompletedTask;
        }
    }
}

