using System.Collections.Generic;

namespace FluffyBunny4.Models
{
    public class SelfHelpUser
    {
        public int Id { get; set; }
        public string ExternalUserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool Enabled { get; set; }
        public ICollection<string> AllowedSelfHelpClients { get; set; } = new HashSet<string>();
    }
}