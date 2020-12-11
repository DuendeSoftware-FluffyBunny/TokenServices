using IdentityModel;
using OIDCPipeline.Core.Extensions;
using OIDCPipeline.Core.Validation.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIDCPipeline.Core.Logging.Models
{
    internal class TokenRequestValidationLog
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string GrantType { get; set; }
        public string Scopes { get; set; }

        public string AuthorizationCode { get; set; }
        public string RefreshToken { get; set; }

        public string UserName { get; set; }
        public IEnumerable<string> AuthenticationContextReferenceClasses { get; set; }
        public string Tenant { get; set; }
        public string IdP { get; set; }

        public Dictionary<string, string> Raw { get; set; }

        private static readonly string[] SensitiveValuesFilter =
        {
            OidcConstants.TokenRequest.ClientSecret,
            OidcConstants.TokenRequest.Password,
            OidcConstants.TokenRequest.ClientAssertion,
            OidcConstants.TokenRequest.RefreshToken,
            OidcConstants.TokenRequest.DeviceCode

        };

        public TokenRequestValidationLog(ValidatedTokenRequest request)
        {
            Raw = request.Raw.ToScrubbedDictionary(SensitiveValuesFilter);
            GrantType = request.GrantType;
            AuthorizationCode = request.AuthorizationCodeHandle;
        }

        public override string ToString()
        {
            return LogSerializer.Serialize(this);
        }
    }
}
