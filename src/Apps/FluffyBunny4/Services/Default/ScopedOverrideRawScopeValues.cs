using System.Collections.Generic;

namespace FluffyBunny4.Services
{
    internal class ScopedOverrideRawScopeValues : IScopedOverrideRawScopeValues
    {
        List<string> _scopes;
        public List<string> Scopes => _scopes ?? (_scopes = new List<string>());

        public bool IsOverride { get; set; }
    }
    
}