using Newtonsoft.Json;
using System;

namespace FluffyBunny4.Models
{
    public class SecretHandle
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "secretMask")]
        public string SecretMasked { get; set; }

        [JsonProperty(PropertyName = "expiration")]
        public DateTime? Expiration { get; set; }
    }
}
