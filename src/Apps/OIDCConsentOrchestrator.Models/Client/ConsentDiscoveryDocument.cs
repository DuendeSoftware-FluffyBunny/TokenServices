using System.Collections.Generic;
using System.Text.Json.Serialization;
using static OIDCConsentOrchestrator.Models.Constants;

namespace OIDCConsentOrchestrator.Models.Client
{
    public class ConsentDiscoveryDocument 
    {
        [JsonPropertyName("authorization_endpoint")]
        public string AuthorizeEndpoint { get; set; }
        [JsonPropertyName("scopes_supported")]
        public List<string> ScopesSupported { get; set; }
        [JsonPropertyName("authorization_type")]
        public string AuthorizationType { get; set; } = AuthorizationTypes.Subject;
    }
}
