using Microsoft.AspNetCore.Mvc;
using OIDCPipeline.Core.Endpoints.ResponseHandling;
using OIDCPipeline.Core.Extensions;

namespace OIDCPipeline.Core.Validation.Models
{
    internal class AuthorizeResponse
    {
        public ValidatedAuthorizeRequest Request { get; set; }
        public string RedirectUri => Request?.RedirectUri;
        public string State => Request?.State;
        public string Scope { get; set; }
        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }
        public int AccessTokenLifetime { get; set; }
        public string Code { get; set; }
        public string SessionState { get; set; }

        public string Error { get; set; }
        public string ErrorDescription { get; set; }
        public bool IsError => Error.IsPresent();

        public DownstreamAuthorizeResponse Downstream { get; internal set; }
    }
}
