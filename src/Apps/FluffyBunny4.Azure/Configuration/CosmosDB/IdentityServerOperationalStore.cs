using Newtonsoft.Json;

namespace FluffyBunny4.Azure.Configuration.CosmosDB
{
    public partial class IdentityServerOperationalStore
    {
        [JsonProperty("database")]
        public string Database { get; set; }

        [JsonProperty("collection")]
        public string Collection { get; set; }
    }
}