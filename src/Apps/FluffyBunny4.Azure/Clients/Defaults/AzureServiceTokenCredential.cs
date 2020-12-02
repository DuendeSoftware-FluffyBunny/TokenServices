using Azure.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluffyBunny4.Azure.Clients

{
    public abstract class AzureServiceTokenCredential<T> : TokenCredential 
        where T:class 
    {
        public AzureServiceTokenCredential(string endPoint,ILogger<T> logger)
        {
            if (string.IsNullOrWhiteSpace(endPoint))
            {
                throw new ArgumentException("message", nameof(endPoint));
            }

            EndPoint = endPoint;
            ArmClientId = Environment.GetEnvironmentVariable("ARM_CLIENT_ID");
            ArmClientSecret = Environment.GetEnvironmentVariable("ARM_CLIENT_SECRET");
            ArmSubscriptionId = Environment.GetEnvironmentVariable("ARM_SUBSCRIPTION_ID");
            ArmTenantId = Environment.GetEnvironmentVariable("ARM_TENANT_ID");
            _logger = logger;
        }

        public string EndPoint { get; }
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
                var credential = new ClientCredential(ArmClientId, ArmClientSecret);
                var tokenProvider = new AzureServiceTokenProvider();
                var authenticationContext = new AuthenticationContext($"https://login.microsoftonline.com/{ArmTenantId}");

                var result = await authenticationContext.AcquireTokenAsync(EndPoint, credential);
                return new AccessToken(result.AccessToken, result.ExpiresOn);
            }
            else
            {
                _logger.LogInformation("AzureServiceTokenCredential is utilizing AzureServiceTokenProvider");

                var tokenProvider = new AzureServiceTokenProvider();
                var accessToken = await tokenProvider
                    .GetAccessTokenAsync(EndPoint, null, cancellationToken)
                    .ContinueWith(task => {
                        return new AccessToken(task.Result, DateTimeOffset.MaxValue);
                    });
                return accessToken;
            }
        }
    }
}
