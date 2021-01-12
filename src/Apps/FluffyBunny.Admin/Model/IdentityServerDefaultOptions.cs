using System.Collections.Generic;

namespace FluffyBunny.Admin.Model
{
    public class IdentityServerDefaultOptions
    {
        public List<string> AvailableGrantTypes { get; set; }
        public List<string> AvailableRevokeTokenTypeHints { get; set; }

    }
}