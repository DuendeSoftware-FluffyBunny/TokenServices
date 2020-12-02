using FluffyBunny4.Azure.Models;
using System;
using Duende.IdentityServer.Models;

namespace FluffyBunny4.Azure.Extensions
{
    public static class PersistedGrantExtensions
    {
        public static PersistedGrant ToPersistedGrant(this PersistedGrantCosmosDocument self)
        {
            if (self == null)
            {
                return null;
            }
            return new PersistedGrant
            {
                ClientId = self.ClientId,
                CreationTime = self.CreationTime,
                Data = self.Data,
                Expiration = self.Expiration,
                Key = self.Id,
                SubjectId = self.SubjectId,
                Type = self.Type
            };
        }

        public static PersistedGrantCosmosDocument ToPersistedGrantCosmosDocument(this PersistedGrant self)
        {
            if (self == null)
            {
                return null;
            }
            int? ttl = null;
            if (self.Expiration != null)
            {
                var diffInSeconds = (int)(((DateTime)self.Expiration - DateTime.UtcNow).TotalSeconds);
                if (diffInSeconds <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(self.Expiration));
                }
                ttl = diffInSeconds;
            }
            return new PersistedGrantCosmosDocument
            {
                Id = self.Key,
                ClientId = self.ClientId,
                CreationTime = self.CreationTime,
                Data = self.Data,
                SubjectId = self.SubjectId,
                Type = self.Type,
                Expiration = self.Expiration,
                ttl = ttl
            };
        }
    }
}
