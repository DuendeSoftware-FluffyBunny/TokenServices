using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Entities
{
    public class PersistedGrantExtra : PersistedGrant
    {
        public string RefreshTokenKey { get; set; }
    }
}
