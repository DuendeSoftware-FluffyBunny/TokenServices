using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FluffyBunny4.Models
{
    public class ConsentAuthorizeResponseContainer<T> where T : class
    {
        public T Context { get; set; }
        public ConsentAuthorizeResponse Response { get; set; }
    }
    public class ConsentAuthorizeResponse : ConsentBaseResponse
    {
        public class ConsentAuthorizeClaim
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }

        [JsonPropertyName("authorized")]
        public bool Authorized { get; set; }
        [JsonPropertyName("scopes")]
        public List<string> Scopes { get; set; }
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("claims")]
        public List<ConsentAuthorizeClaim> Claims { get; set; }

        [JsonPropertyName("custom_payload")]
        public object CustomPayload { get; set; }
    }
}
