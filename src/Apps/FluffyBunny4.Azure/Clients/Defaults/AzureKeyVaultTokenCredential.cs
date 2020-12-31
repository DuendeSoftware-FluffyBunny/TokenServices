using Microsoft.Extensions.Logging;

namespace FluffyBunny4.Azure.Clients
{
    public class AzureKeyVaultTokenCredential : AzureServiceTokenCredential<AzureKeyVaultTokenCredential>
    {
        public AzureKeyVaultTokenCredential(ILogger<AzureKeyVaultTokenCredential> logger) : 
            base("https://vault.azure.net/.default", logger)
        {
        }
    }
}
