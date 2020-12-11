using OIDCPipeline.Core.Endpoints.ResponseHandling;
using System.Collections.Generic;

namespace OIDCPipeline.Core.Validation.Models
{
    public class ValidatedTokenRequest : ValidatedRequest
    {
        /// <summary>
        /// Gets or sets the type of the grant.
        /// </summary>
        /// <value>
        /// The type of the grant.
        /// </value>
        public string GrantType { get; set; }
 
        /// <summary>
        /// Gets or sets the authorization code.
        /// </summary>
        /// <value>
        /// The authorization code.
        /// </value>
        public DownstreamAuthorizeResponse IdTokenResponse { get; set; }

        /// <summary>
        /// Gets or sets the authorization code handle.
        /// </summary>
        /// <value>
        /// The authorization code handle.
        /// </value>
        public string AuthorizationCodeHandle { get; set; }

        /// <summary>
        /// Gets or sets the code verifier.
        /// </summary>
        /// <value>
        /// The code verifier.
        /// </value>
        public string CodeVerifier { get; set; }
       
        public string ClientId { get; internal set; }
    }
}
