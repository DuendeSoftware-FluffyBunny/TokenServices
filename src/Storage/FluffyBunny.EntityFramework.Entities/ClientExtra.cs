using System.Collections.Generic;
using Duende.IdentityServer.EntityFramework.Entities;

namespace FluffyBunny.EntityFramework.Entities
{
    public class ClientExtra: Client
    {
        public bool AllowGlobalSubjectRevocation { get; set; } = true;
        public bool RefreshTokenGraceEnabled { get; set; } = true;
        public int RefreshTokenGraceTTL { get; set; } = 1800;
        public int RefreshTokenGraceMaxAttempts { get; set; } = 10;
        public bool RequireRefreshClientSecret { get; set; } = false;
        public bool IncludeClientId { get; set; } = false;
        public bool IncludeAmr { get; set; } = false;
        public string TenantName { get; set; }
        public List<AllowedArbitraryIssuer> AllowedArbitraryIssuers { get; set; }
        public List<AllowedRevokeTokenTypeHint> AllowedRevokeTokenTypeHints { get; set; }
        public List<AllowedTokenExchangeExternalService> AllowedTokenExchangeExternalServices { get; set; }
        

       public string Namespace { get; set; }
    }
}
