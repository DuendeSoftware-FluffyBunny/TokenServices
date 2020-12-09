using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;

namespace FluffyBunny4.Models
{
    public class PersistedGrantExtra: PersistedGrant
    {
        public string RefreshTokenKey { get; set; }
        public string AccessTokenKey { get; set; }
    }
}
