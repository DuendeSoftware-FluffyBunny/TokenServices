using System;
using System.Collections.Generic;
using System.Text;

namespace OIDCPipeline.Core.Configuration
{
    public class MemoryCacheOIDCPipelineStoreOptions
    {
        public int ExpirationMinutes { get; set; } = 30;
    }
    public class InputLengthRestrictions
    {
        private const int Default = 100;

        /// <summary>
        /// Max length for redirect_uri
        /// </summary>
        public int RedirectUri { get; set; } = 400;

        /// <summary>
        /// Max length for nonce
        /// </summary>
        public int Nonce { get; set; } = 300;
        /// <summary>
        /// Min length for the code challenge
        /// </summary>
        public int CodeChallengeMinLength { get; } = 43;

        /// <summary>
        /// Max length for the code challenge
        /// </summary>
        public int CodeChallengeMaxLength { get; } = 128;

        /// <summary>
        /// Min length for the code verifier
        /// </summary>
        public int CodeVerifierMinLength { get; } = 43;

        /// <summary>
        /// Max length for the code verifier
        /// </summary>
        public int CodeVerifierMaxLength { get; } = 128;

        /// <summary>
        /// Max length for grant_type
        /// </summary>
        public int GrantType { get; set; } = Default;

        /// <summary>
        /// Max length for authorization codes
        /// </summary>
        public int AuthorizationCode { get; set; } = Default;
    }

    
}
