using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluffyBunny4.Models
{
    public partial class ApiResourceRecord
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("scopeNameSpace")]
        public string ScopeNameSpace { get; set; }

        [JsonProperty("scopes")]
        public List<ApiResourceScope> Scopes { get; set; }
        [JsonProperty("secrets")]
        public List<string> Secrets { get; set; }
    }
}
