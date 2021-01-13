using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FluffyBunny4.Models
{

    public class ConsentAuthorizeRequest
    {
        public ConsentAuthorizeRequest()
        {
            Requester = new ClientRequester();
        }
        public class ClientRequester
        {
            public string ClientId { get; set; }
            public string ClientName { get; set; }
            public string ClientDescription { get; set; }
            public string Namespace { get; set; }
            public string Tenant { get; set; }
        }
        [JsonPropertyName("requester")]
        public ClientRequester Requester { get; set; }

        [JsonPropertyName("authorization_type")]
        public string AuthorizeType { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("scopes")]
        public List<string> Scopes { get; set; }

 
    }
}
