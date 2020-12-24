using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FluffyBunny4.Models
{
    public class ConsentAuthorizeRequest
    {
        [JsonPropertyName("authorization_type")]
        public string AuthorizeType { get; set; }
        [JsonPropertyName("subject")]
        public string Subject { get; set; }
        [JsonPropertyName("scopes")]
        public List<string> Scopes { get; set; }
 
    }
}
