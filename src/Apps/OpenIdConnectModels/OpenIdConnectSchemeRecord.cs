
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenIdConnectModels
{ 
    /*
     Managed Secrets
        {
          "oauth2:0:clientId": "<blah>.apps.googleusercontent.com",
          "oauth2:0:clientSecret": "<blah>"
        }

    "oauth2": [
        {
            "scheme": "Google",
            "clientId": "<put in manage user secrets>",
            "clientSecret": "<put in manage user secrets>",
            "authority": "https://accounts.google.com",
            "callbackPath": "/signin-google",
            "additionalEndpointBaseAddresses": []
        }
    ]
    */
    public class OpenIdConnectSchemeRecord
    {
        public string Scheme { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Authority { get; set; }
        public string CallbackPath { get; set; }
        public List<string> AdditionalEndpointBaseAddresses { get; set; }
        public List<string> AdditionalProtocolScopes { get; set; }

        public string ResponseType { get; set; }
        public bool GetClaimsFromUserInfoEndpoint { get; set; }
        
    }
}
