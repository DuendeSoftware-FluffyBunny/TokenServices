using System.Collections.Generic;
using IdentityModel;
using OIDCPipeline.Core.Extensions;
using OIDCPipeline.Core.Validation.Models;

namespace OIDCPipeline.Core.Logging.Models
{
    internal class AuthorizeRequestValidationLog
    {
        public AuthorizeRequestValidationLog(ValidatedAuthorizeRequest request)
        {
            Raw = request.Raw.ToScrubbedDictionary(OidcConstants.AuthorizeRequest.IdTokenHint);
            Nonce = request.Nonce;
            RedirectUri = request.RedirectUri;
            ResponseType = request.ResponseType;
            ResponseMode = request.ResponseMode;
            GrantType = request.GrantType;
        }

        public Dictionary<string, string> Raw { get; }
        public string Nonce { get; }
        public string RedirectUri { get; }
        public string ResponseType { get; }
        public string ResponseMode { get; }
        public string GrantType { get; }

        public override string ToString()
        {
            return LogSerializer.Serialize(this);
        }
    }
}
