using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyBunny.EntityFramework.Entities
{
    public class SelfHelpUser
    {
        public int Id { get; set; }
        public string ExternalUserId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public List<AllowedSelfHelpClient> AllowedSelfHelpClients { get; set; }

    }
}
