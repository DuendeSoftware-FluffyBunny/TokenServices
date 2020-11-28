using System.Collections.Generic;
using Duende.IdentityServer.Models;

namespace FluffyBunny4.Models
{
    public class JwksDiscoveryDocument
    {
        public List<JsonWebKey> Keys { get; set; }
    }
}
