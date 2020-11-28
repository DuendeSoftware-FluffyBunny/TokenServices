using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Security.KeyVault.Keys;
using System.Collections.Generic;
using Duende.IdentityServer.Models;

namespace FluffyBunny4.Models
{
    public class ECDsaKeyCache
    {
        public KeyProperties CurrentKeyProperties { get; set; }
        public CryptographyClient CryptographyClient { get; set; }
        public List<SecurityKeyInfo> SecurityKeyInfos { get; set; }
    }
}
