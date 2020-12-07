using System.Collections.Generic;
using System.Security.Claims;

namespace FluffyBunny4.Services
{
    internal class ScopedOptionalClaims : IScopedOptionalClaims
    {
        List<Claim> _claims;
        public List<Claim> Claims => _claims ?? (_claims = new List<Claim>());
    }
}
