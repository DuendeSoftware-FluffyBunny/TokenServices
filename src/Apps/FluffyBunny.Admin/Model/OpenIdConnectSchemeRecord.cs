﻿using System.Collections.Generic;

namespace FluffyBunny.Admin.Model
{
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
        public string DisplayNameClaimName { get; set; }

    }
}
