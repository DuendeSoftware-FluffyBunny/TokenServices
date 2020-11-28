using Newtonsoft.Json;

namespace FluffyBunny4.Models
{
    public partial class ApiResourceScope
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }
}
