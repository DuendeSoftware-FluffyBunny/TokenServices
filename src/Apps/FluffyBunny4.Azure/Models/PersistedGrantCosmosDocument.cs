using Newtonsoft.Json;
using System;

namespace FluffyBunny4.Azure.Models
{
    public class PersistedGrantCosmosDocument  
    {
        public PersistedGrantCosmosDocument() { }
        public PersistedGrantCosmosDocument(PersistedGrantCosmosDocument other)
        {
            Id = other.Id;
            Type = other.Type;
            SubjectId = other.SubjectId;
            TenantId = other.TenantId;
            ClientId = other.ClientId;
            CreationTime = other.CreationTime;
            Expiration = other.Expiration;
            Data = other.Data;
            ttl = other.ttl;
        }
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "subjectId")]
        public string SubjectId { get; set; }
        
        [JsonProperty(PropertyName = "tenantId")]
        public string TenantId { get; set; }

        [JsonProperty(PropertyName = "clientId")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "creationTime")]
        public DateTime CreationTime { get; set; }

        [JsonProperty(PropertyName = "expiration")]
        public DateTime? Expiration { get; set; }

        [JsonProperty(PropertyName = "ttl", NullValueHandling = NullValueHandling.Ignore)]
        public int? ttl { get; set; }

        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }
    }
}
