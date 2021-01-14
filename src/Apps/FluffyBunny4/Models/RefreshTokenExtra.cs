using System;
using Duende.IdentityServer.Models;

namespace FluffyBunny4.Models
{
    public class RefreshTokenExtra : RefreshToken
    {
        public string RefeshTokenParent { get; set; }
        public string RefeshTokenChild { get; set; }
        public int ConsumedAttempts { get; set; } = 0;
        public string OriginGrantType { get; set; }
    }
}
