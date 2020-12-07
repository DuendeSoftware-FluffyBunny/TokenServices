using System.Collections.Generic;

namespace FluffyBunny4.Services
{
    public interface IScopedOverrideRawScopeValues
    {
        List<string> Scopes { get; }
    }
}