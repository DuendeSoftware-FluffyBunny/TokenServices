using System.Collections.Generic;

namespace FluffyBunny4.Services
{
    internal class OverrideRawScopeValues : IOverrideRawScopeValues
    {
        List<string> _scopes;
        public List<string> Scopes => _scopes ?? (_scopes = new List<string>());
    }
}