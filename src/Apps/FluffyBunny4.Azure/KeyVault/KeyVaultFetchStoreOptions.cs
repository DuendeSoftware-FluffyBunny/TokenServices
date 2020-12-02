namespace FluffyBunny4.Azure.KeyVault
{
    public class KeyVaultFetchStoreOptions<T>
          where T : class
    {
        public T Value;
        public int ExpirationSeconds { get; set; } = 60;
        public string KeyVaultName { get; set; } = "{your-KeyVaultName}"; // https://{your-KeyVaultName}.vault.azure.net/
        public string KeyVaultUrl
        {
            get { return $"https://{KeyVaultName}.vault.azure.net/"; }
        }
        public string SecretName { get; set; } = "{your-SecretName}"; // https://{your-kv-name}.vault.azure.net/secrets/{your-SecretName}
        public string KeyVaultSecretUrl
        {
            get { return $"{KeyVaultUrl}secrets/{SecretName}"; }
        }
        public string Schema { get; set; }
    }
}
