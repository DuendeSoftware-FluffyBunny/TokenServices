using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FluffyBunny4.Azure.KeyVault
{
    public class SimpleStringKeyVaultFetchStore : KeyVaultFetchStore<string>
    {
        public SimpleStringKeyVaultFetchStore(
                  KeyVaultFetchStoreOptions<string> options, KeyVaultClient keyVaultClient, ILogger logger) :
                  base(options, keyVaultClient, logger)
        {
        }

        public SimpleStringKeyVaultFetchStore(
            IOptions<KeyVaultFetchStoreOptions<string>> options, KeyVaultClient keyVaultClient, ILogger logger) :
            base(options, keyVaultClient, logger)
        {
        }
        public async Task<string> GetStringValueAsync()
        {
            await SafeFetchAsync();
            return Value;
        }
        protected override void OnRefresh()
        {

        }
        protected override string DeserializeValue(string raw)
        {
            return raw;
        }
        async Task SafeFetchAsync()
        {
            await GetValueAsync();
        }
    }
}
