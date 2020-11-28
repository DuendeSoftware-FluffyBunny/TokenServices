using Newtonsoft.Json;

namespace FluffyBunny4.Models
{
    public class ClaimHandle
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}
