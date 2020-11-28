using IdentityModel.Client;
using System;

namespace IdentityModel.FluffyBunny4
{
    //
    // Summary:
    //     Request for token using client_credentials
    public class ArbitraryTokenTokenRequest : TokenRequest
    {
        public ArbitraryTokenTokenRequest() { }

        //
        // Summary:
        //     Space separated list of the requested scopes
        //
        // Value:
        //     The scope.
        public string? Scope { get; set; }
        public string? ArbitraryClaims { get; set; }
        public string? Subject { get; set; }
        public int? AccessTokenLifetime { get; set; }
        public string? ArbitraryAmrs { get; set; }
        public string? ArbitraryAudiences { get; set; }
        public string? CustomPayload { get; set; }
    }
}
