using Azure.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Identity.Client;
using ClientCredential = Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential;

namespace FluffyBunny4.Azure.Clients

{
    public abstract class AzureServiceTokenCredential<T> : TokenCredential 
        where T:class 
    {
        
        public AzureServiceTokenCredential(string scope,ILogger<T> logger)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException("scope is required", nameof(scope));
            }

            _scope = scope;
            ArmClientId = Environment.GetEnvironmentVariable("ARM_CLIENT_ID");
            ArmClientSecret = Environment.GetEnvironmentVariable("ARM_CLIENT_SECRET");
            ArmSubscriptionId = Environment.GetEnvironmentVariable("ARM_SUBSCRIPTION_ID");
            ArmTenantId = Environment.GetEnvironmentVariable("ARM_TENANT_ID");
            _logger = logger;
        }

      

        private string _scope;

        public string ArmClientId { get; }
        public string ArmClientSecret { get; }
        public string ArmSubscriptionId { get; }
        public string ArmTenantId { get; }

        private ILogger _logger;

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var token = GetTokenAsync(requestContext, cancellationToken).GetAwaiter().GetResult();
            return token;
        }

        public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            if (
                !string.IsNullOrWhiteSpace(ArmClientId) ||
                !string.IsNullOrWhiteSpace(ArmClientSecret) ||
                !string.IsNullOrWhiteSpace(ArmSubscriptionId) ||
                !string.IsNullOrWhiteSpace(ArmTenantId)
            )
            {
                _logger.LogInformation("AzureServiceTokenCredential Utilizing ARM_* Environment Variables");
                var instance = "https://login.microsoftonline.com/";

                var conClient = ConfidentialClientApplicationBuilder.Create(ArmClientId)
                    .WithAuthority($"{instance}{ArmTenantId}")
                    .WithClientSecret(ArmClientSecret)
                    .Build();

                var clientResult = await conClient.AcquireTokenForClient(new[] {_scope}).ExecuteAsync();
              
                /*
                var clientResult = conClient.AcquireTokenForClient(
                        new[] { _scope })
                    .ExecuteAsync().Result;
                
                var kc = new KeyVaultCredential((authority, resource, scope) =>
                {
                    Console.WriteLine($"Authority: {authority}, Resource: {resource}, Scope: {scope}");
                    return Task.FromResult(clientResult.AccessToken);
                });

                var kvClient = new KeyVaultClient(kc);
            */

                return new AccessToken(clientResult.AccessToken, clientResult.ExpiresOn);
            }
            else
            {
                _logger.LogInformation("AzureServiceTokenCredential is utilizing AzureServiceTokenProvider");

                var tokenProvider = new AzureServiceTokenProvider();
                var accessToken = await tokenProvider
                    .GetAccessTokenAsync(_scope, null, cancellationToken)
                    .ContinueWith(task => {
                        return new AccessToken(task.Result, DateTimeOffset.MaxValue);
                    });
                return accessToken;
            }
        }
    }
}
