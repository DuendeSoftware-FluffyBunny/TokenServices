using Newtonsoft.Json;

namespace FluffyBunny4.Azure.Configuration.CosmosDB
{
    public partial class CosmosConfiguration
    {
        [JsonProperty("useCosmos")]
        public bool UseCosmos { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("primaryKey")]
        public string PrimaryKey { get; set; }

        [JsonProperty("primaryConnectionString")]
        public string PrimaryConnectionString { get; set; }

        [JsonProperty("identityServerOperationalStore")]
        public IdentityServerOperationalStore IdentityServerOperationalStore { get; set; }
    }
}