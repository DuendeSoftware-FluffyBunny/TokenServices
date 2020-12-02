using FluffyBunny4.Azure.KeyVault;
using FluffyBunny4.Azure.Utils;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FluffyBunny4.Services
{
    public class KeyVaultSecretProvider : IKeyVaultSecretProvider
    {
        private KeyVaultClient _keyVaultClient;
        private ILogger _logger;

        public KeyVaultSecretProvider(
            KeyVaultClient keyVaultClient,
            ILogger<KeyVaultSecretProvider> logger)
        {
            _keyVaultClient = keyVaultClient;
            _logger = logger;
        }


        public async Task<string> GetSecretAsync(string keyVaultName, string secretName)
        {
            try
            {
                var kvFetchStore = new SimpleStringKeyVaultFetchStore(
                     new KeyVaultFetchStoreOptions<string>()
                     {
                         ExpirationSeconds = 3600,
                         KeyVaultName = keyVaultName,
                         SecretName = secretName
                     }, _keyVaultClient, _logger);
                var secret = await kvFetchStore.GetStringValueAsync();
                return secret;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,ex.Message);
            }
            return null;
        }
    }
}
