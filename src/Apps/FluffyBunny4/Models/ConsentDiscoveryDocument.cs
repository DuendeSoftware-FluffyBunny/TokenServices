using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FluffyBunny4.Models
{
    public class ConsentDiscoveryDocument
    {
        [JsonPropertyName("authorization_endpoint")]
        public string AuthorizeEndpoint { get; set; }
        [JsonPropertyName("scopes_supported")]
        public List<string> ScopesSupported { get; set; }
        [JsonPropertyName("authorization_type")]
        public string AuthorizationType { get; set; } = Constants.AuthorizationTypes.SubjectAndScopes;
    }
}
