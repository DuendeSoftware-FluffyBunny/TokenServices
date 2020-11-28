using FluffyBunny4.Extensions;
using FluffyBunny4.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using Duende.IdentityServer.Models;

namespace FluffyBunny4.Extensions
{
    public static class ClientExtraExtensions
    {
        public static Secret ToSecret(this SecretHandle self)
        {
            return new Secret(self.Value, self.Description, self.Expiration);
        }
        public static SecretHandle ToSecretHandle(this Secret self)
        {
            return new SecretHandle()
            {
                Description = self.Description,
                Value = self.Value,
                Expiration = self.Expiration
            };
        }
        public static List<Secret> ToSecrets(this List<SecretHandle> self)
        {
            var query = from item in self
                        select item.ToSecret();
            return query.ToList();
        }

        public static List<SecretHandle> ToSecretHandles(this List<Secret> self)
        {
            var query = from item in self
                        select item.ToSecretHandle();
            return query.ToList();
        }
        public static List<SecretHandle> ToSecretHandles(this ICollection<Secret> self)
        {
            var query = from item in self
                        select item.ToSecretHandle();
            return query.ToList();
        }
        public static ClientClaim ToClientClaim(this ClaimHandle self)
        {
            return new ClientClaim(self.Type, self.Value);
        }
        public static List<ClientClaim> ToClientClaims(this List<ClaimHandle> self)
        {
            var query = from item in self
                        select item.ToClientClaim();
            return query.ToList();
        }


        public static ClaimHandle ToClaimHandle(this ClientClaim self)
        {
            return new ClaimHandle()
            {
                Type = self.Type,
                Value = self.Value
            };
        }
        public static List<ClaimHandle> ToClaimHandles(this List<ClientClaim> self)
        {
            var query = from item in self
                        select item.ToClaimHandle();
            return query.ToList();
        }
        public static List<ClaimHandle> ToClaimHandles(this ICollection<ClientClaim> self)
        {
            var query = from item in self
                        select item.ToClaimHandle();
            return query.ToList();
        }
    }
}
