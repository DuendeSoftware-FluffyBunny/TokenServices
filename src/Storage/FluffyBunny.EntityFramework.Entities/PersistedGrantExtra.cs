using Duende.IdentityServer.EntityFramework.Entities;

namespace FluffyBunny.EntityFramework.Entities
{
    public class PersistedGrantExtra : PersistedGrant
    {
        public string RefreshTokenKey { get; set; }
    }
}
