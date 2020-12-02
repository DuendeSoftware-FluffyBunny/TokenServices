using FluffyBunny4.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FluffyBunny4.Azure.Models
{
    public partial class ClientApiCointainter
    {
        [JsonProperty("clients")]
        public Dictionary<string, ClientRecord> ClientRecords { get; set; }
        [JsonProperty("apiResources")]
        public List<ApiResourceRecord> ApiResourceRecords { get; set; }
    }
}
