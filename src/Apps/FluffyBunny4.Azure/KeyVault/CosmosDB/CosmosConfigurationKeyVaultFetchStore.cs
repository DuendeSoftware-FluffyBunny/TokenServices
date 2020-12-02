using FluffyBunny4.Azure.Configuration.CosmosDB;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FluffyBunny4.Azure.KeyVault.CosmosDB
{
    public class CosmosConfigurationKeyVaultFetchStore : KeyVaultFetchStore<CosmosConfiguration>
    {
        public CosmosConfigurationKeyVaultFetchStore(
                   KeyVaultFetchStoreOptions<CosmosConfiguration> options, KeyVaultClient keyVaultClient, ILogger logger) :
                   base(options, keyVaultClient, logger)
        {
        }

        public CosmosConfigurationKeyVaultFetchStore(
            IOptions<KeyVaultFetchStoreOptions<CosmosConfiguration>> options, KeyVaultClient keyVaultClient, ILogger logger) :
            base(options, keyVaultClient, logger)
        {
        }
        public async Task<CosmosConfiguration> GetConfigurationAsync()
        {
            await SafeFetchAsync();
            return Value;
        }
        protected override void OnRefresh()
        {

        }
        async Task SafeFetchAsync()
        {
            await GetValueAsync();
        }
    }
}
