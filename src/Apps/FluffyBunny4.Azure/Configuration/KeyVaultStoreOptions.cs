using System;
using System.Collections.Generic;
using System.Text;

namespace FluffyBunny4.Configuration
{
    public class KeyVaultStoreOptions : ICloneable
    {
        public int ExpirationSeconds { get; set; } = 86400;// one day
        public string KeyVaultName { get; set; } = "{your-KeyVaultName}"; // https://{your-KeyVaultName}.vault.azure.net/
        public string KeyVaultUrl
        {
            get { return $"https://{KeyVaultName}.vault.azure.net/"; }
        }
        public string KeyIdentifier { get; set; } = "{your-KeyIdentifier}";

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
