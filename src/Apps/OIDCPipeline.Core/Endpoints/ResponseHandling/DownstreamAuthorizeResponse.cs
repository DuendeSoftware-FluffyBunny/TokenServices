using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.Json.Serialization;
using OIDCPipeline.Core.Validation.Models;

namespace OIDCPipeline.Core.Endpoints.ResponseHandling
{
    public class DownstreamAuthorizeResponse
    {
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public string ExpiresAt { get; set; }
        public string LoginProvider { get; set; }
        public ValidatedAuthorizeRequest Request { get; set; }
        Dictionary<string, object> _custom;

        public Dictionary<string, object> Custom
        {
            get
            {
                if (_custom == null)
                {
                    _custom = new Dictionary<string, object>();
                }
                return _custom;
            }
            set { _custom = value; }
        }
    }
}
