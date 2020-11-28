using FluffyBunny4.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using Duende.IdentityServer.Models;

namespace FluffyBunny4.Extensions
{
    public static class ClientRecordExtensions
    {
        public static List<Client> LoadClientsFromSettings(this IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetSection("clients");
            var clientRecords = new Dictionary<string, ClientRecord>();
            section.Bind(clientRecords);
            foreach (var clientRecord in clientRecords)
            {
                clientRecord.Value.ClientId = clientRecord.Key;
            }
            var clients = clientRecords.ToClients();
            return clients;
        }
        public static Client ToClient(this ClientRecord self)
        {
            List<Secret> secrets = new List<Secret>();
            foreach (var secret in self.Secrets)
            {
                secrets.Add(new Secret(secret.Sha256()));
            }
            var clientExtra = new ClientExtra()
            {
                ClientId = self.ClientId,
                IdentityTokenLifetime = self.IdentityTokenLifetime,
                AbsoluteRefreshTokenLifetime = self.AbsoluteRefreshTokenLifetime,
                AccessTokenLifetime = self.AccessTokenLifetime,
                AllowedGrantTypes = self.AllowedGrantTypes,
                AllowedScopes = self.AllowedScopes,
                AllowOfflineAccess = self.AllowOfflineAccess,
                AlwaysSendClientClaims = self.AlwaysSendClientClaims,
                AccessTokenType = (AccessTokenType)self.AccessTokenType,
                ClientClaimsPrefix = self.ClientClaimsPrefix,
                ClientSecrets = secrets,
                Enabled = self.Enabled,
                FrontChannelLogoutSessionRequired = self.FrontChannelLogoutSessionRequired,
                FrontChannelLogoutUri = self.FrontChannelLogoutUri,
                PostLogoutRedirectUris = self.PostLogoutRedirectUris,
                RedirectUris = self.RedirectUris,
                RefreshTokenUsage = (TokenUsage)self.RefreshTokenUsage,
                RequireClientSecret = self.RequireClientSecret,
                RequireConsent = self.RequireConsent,
                RequireRefreshClientSecret = self.RequireRefreshClientSecret,
                SlidingRefreshTokenLifetime = self.SlidingRefreshTokenLifetime,
                IncludeJwtId = self.IncludeJwtId,
                TenantId = self.TenantId,
                IncludeClientId = self.IncludeClientId,
                AllowGlobalSubjectRevocation = self.AllowGlobalSubjectRevocation,
                RefreshTokenGraceEnabled = self.RefreshTokenGraceEnabled,
                RefreshTokenGraceTTL = self.RefreshTokenGraceTTL,
                RefreshTokenGraceMaxAttempts = self.RefreshTokenGraceMaxAttempts
            };
            if (self.ClaimHandles.Any())
            {
                var queryClaims = from claimHandle in self.ClaimHandles
                                  let c = new ClientClaim(claimHandle.Type, claimHandle.Value)
                                  select c;
                clientExtra.Claims = queryClaims.ToList();
            }
            return clientExtra;
        }

        public static List<Client> ToClients(this Dictionary<string, ClientRecord> self)
        {
            List<Client> result = new List<Client>();
            foreach (var clientRecord in self)
            {
                clientRecord.Value.ClientId = clientRecord.Key;
                var client = clientRecord.Value.ToClient();
                result.Add(client);
            }

            return result;
        }

    }
}
