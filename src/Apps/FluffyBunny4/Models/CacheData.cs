using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace FluffyBunny4.Models
{
    public class CacheData
    {
        public List<RsaSecurityKey> RsaSecurityKeys { get; set; }
        public SigningCredentials SigningCredentials { get; set; }
        public List<JsonWebKey> JsonWebKeys { get; set; }
        public KeyIdentifier KeyIdentifier { get; set; }
        private string _kidHash;
        public string KidHash
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_kidHash))
                {
                    var idx = KeyIdentifier.Identifier.LastIndexOf('/');
                    _kidHash = KeyIdentifier.Identifier.Substring(idx + 1);
                }
                return _kidHash;
            }
        }
        public X509Certificate2 X509Certificate2 { get; set; }
    }
}
