using Azure.Identity;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FluffyBunny4.Azure.Utils
{
    public static class ManagedIdentityHelper
    {
        public static KeyVaultClient CreateKeyVaultClient(ILogger logger = null)
        {
            var armClientId = Environment.GetEnvironmentVariable("ARM_CLIENT_ID");
            var armClientSecret = Environment.GetEnvironmentVariable("ARM_CLIENT_SECRET");
            var armSubscriptionId = Environment.GetEnvironmentVariable("ARM_SUBSCRIPTION_ID");
            var armTenantId = Environment.GetEnvironmentVariable("ARM_TENANT_ID");
            KeyVaultClient keyVaultClient;
            if (
                string.IsNullOrWhiteSpace(armClientId) ||
                string.IsNullOrWhiteSpace(armClientSecret) ||
                string.IsNullOrWhiteSpace(armSubscriptionId) ||
                string.IsNullOrWhiteSpace(armTenantId)
            )
            {
                if (logger != null)
                {
                    logger.LogInformation("CreateKeyVaultClient is utilizing ARM_* Environment Variables");
                }
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var authCallback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
                keyVaultClient = new KeyVaultClient(authCallback);
            }
            else
            {
                if (logger != null)
                {
                    logger.LogInformation("CreateKeyVaultClient is utilizing AuthenticationContext.AcquireTokenAsync Technique");
                }
                keyVaultClient = new KeyVaultClient(async (authority, resource, scope) =>
                {
                    var adCredential = new Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential(armClientId, armClientSecret);
                    var authenticationContext = new AuthenticationContext(authority, null);
                    return (await authenticationContext.AcquireTokenAsync(resource, adCredential)).AccessToken;
                });

               
            }
            return keyVaultClient;
        }
    }
}
