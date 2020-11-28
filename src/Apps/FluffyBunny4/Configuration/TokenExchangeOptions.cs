using System;
using System.Collections.Generic;
using System.Text;

namespace FluffyBunny4.Configuration
{
    public class TokenExchangeOptions
    {
        public string BaseScope { get; set; }
        public Dictionary<string, string> Authorities { get; set; }
        public string AuthorityKey { get; set; }
        
    }
}
