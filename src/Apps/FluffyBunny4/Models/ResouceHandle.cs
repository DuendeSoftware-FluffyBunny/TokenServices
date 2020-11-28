using System.Collections.Generic;

namespace FluffyBunny4.Models
{
    public class ResouceHandle
    {
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool ShowInDiscoveryDocument { get; set; }
        public List<string> UserClaims { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}
