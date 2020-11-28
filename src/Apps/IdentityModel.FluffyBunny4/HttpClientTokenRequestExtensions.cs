using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.FluffyBunny4
{
    /// <summary>
    /// HttpClient extensions for OAuth token requests
    /// </summary>
    public static class HttpClientTokenRequestExtensions
    {
        /// <summary>
        /// Sends a token request using the client_credentials grant type.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<TokenResponse> RequestArbitraryTokenTokenAsync(
            this HttpMessageInvoker client, ArbitraryTokenTokenRequest request, CancellationToken cancellationToken = default)
        {
            var clone = request.Clone<TokenRequest>();

            clone.Parameters.AddRequired(OidcConstants.TokenRequest.GrantType, "arbitrary_token");
            clone.Parameters.AddOptional(OidcConstants.TokenRequest.Scope, request.Scope);
            clone.Parameters.AddOptional("subject", request.Subject);
            clone.Parameters.AddOptional("arbitrary_claims", request.ArbitraryClaims);
            clone.Parameters.AddOptional("access_token_lifetime", request.AccessTokenLifetime.ToString());
            clone.Parameters.AddOptional("arbitrary_amrs", request.ArbitraryAmrs);
            clone.Parameters.AddOptional("arbitrary_audiences", request.ArbitraryAudiences);
            clone.Parameters.AddOptional("custom_payload", request.CustomPayload);

            return await client.RequestTokenAsync(clone, cancellationToken);
        }

    }
}
