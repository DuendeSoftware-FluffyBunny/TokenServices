using IdentityModel.Client;
using System.Collections.Generic;

namespace IdentityModel.FluffyBunny4
{
    public class ArbitraryTokenTokenRequestV2  : TokenRequest
      
    {
        public ArbitraryTokenTokenRequestV2() { }

        public HashSet<string>? Scope { get; set; }

        public Dictionary<string,List<string>>? ArbitraryClaims { get; set; }

        public string? Subject { get; set; }
        public int? AccessTokenLifetime { get; set; }
        public List<string>? ArbitraryAmrs { get; set; }
        public List<string>? ArbitraryAudiences { get; set; }
        public object? CustomPayload { get; set; }
   
    }
}
