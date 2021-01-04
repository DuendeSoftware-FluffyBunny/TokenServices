using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;

namespace FluffyBunny4.Models
{
    public class DeviceCodeExtra: DeviceCode
    {
        public IEnumerable<Claim> AuthorizedClaims { get; set; }
        public string Issuer { get; set; }
    }
}
