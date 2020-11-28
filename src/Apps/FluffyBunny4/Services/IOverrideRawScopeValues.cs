using System.Collections.Generic;

namespace FluffyBunny4.Services
{
    public interface IOverrideRawScopeValues
    {
        List<string> Scopes { get; }
    }
}