using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Threading.Tasks;

namespace FluffyBunny4.Azure.Clients
{
    public class AzureKeyVaultClients : IAzureKeyVaultClients
    {
        private AzureKeyVaultTokenCredential _azureKeyVaultTokenCredential;

        public AzureKeyVaultClients(AzureKeyVaultTokenCredential azureKeyVaultTokenCredential)
        {
            _azureKeyVaultTokenCredential = azureKeyVaultTokenCredential;
        }

        public SecretClient CreateSecretClient(string keyVaultUrl)
        {
            return new SecretClient(vaultUri: new Uri(keyVaultUrl), credential: _azureKeyVaultTokenCredential);

        }

        public KeyClient CreateKeyClient(string keyVaultUrl)
        {
            return new KeyClient(vaultUri: new Uri(keyVaultUrl), credential: _azureKeyVaultTokenCredential);
        }

        public async Task<CryptographyClient> CreateCryptographyClientAsync(KeyClient keyClient, string keyName, string version = null)
        {
            KeyVaultKey key = await keyClient.GetKeyAsync(keyName, version);
            var cryptoClient = new CryptographyClient(keyId: key.Id, credential: _azureKeyVaultTokenCredential);
            return cryptoClient;
        }
    }
}
