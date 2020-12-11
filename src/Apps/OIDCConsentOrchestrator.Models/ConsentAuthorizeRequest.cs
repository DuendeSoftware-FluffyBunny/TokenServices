using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace OIDCConsentOrchestrator.Models
{
    public class ConsentAuthorizeRequest
    {
        [JsonPropertyName("authorization_type")]
        public string AuthorizeType { get; set; }
        [JsonPropertyName("subject")]
        public string Subject { get; set; }
        [JsonPropertyName("scopes")] 
        public List<string> Scopes { get; set; }

        [JsonPropertyName("ip_address")]
        public string IPAddress { get; set; }

    }
}
 