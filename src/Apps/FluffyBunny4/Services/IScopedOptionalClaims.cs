using System.Collections.Generic;
using System.Security.Claims;

namespace FluffyBunny4.Services
{
    public interface IScopedOptionalClaims
    {
        List<Claim> Claims { get; }
    }
}
