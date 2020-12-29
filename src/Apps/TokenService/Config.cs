// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluffyBunny4;
using FluffyBunny4.Models;
using System.Collections.Generic;
using Duende.IdentityServer.Models;

namespace Duende.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };


        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("api1", "My API")
            };
        public static ICollection<string> ResourceOwnerPassword2 =>
           new[] { GrantType.ClientCredentials, GrantType.ResourceOwnerPassword, Constants.GrantType.ArbitraryToken };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                ///////////////////////////////////////////
                // Console Resource Owner Flow Sample
                //////////////////////////////////////////
                new ClientExtra
                {
                    ClientId = "roclient",
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedGrantTypes = ResourceOwnerPassword2,

                    AllowOfflineAccess = true,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "api1", "api2", "api4.with.roles"
                    },
                    AbsoluteRefreshTokenLifetime = 3600,
                    RefreshTokenGraceEnabled = true,
                    RefreshTokenGraceMaxAttempts = 10,
                    RefreshTokenGraceTTL = 300,

                    RequireRefreshClientSecret = false,
                    IncludeClientId = false,
                    IncludeAmr = false,
                    TenantName = "zeke"
                }
            };
    }
}