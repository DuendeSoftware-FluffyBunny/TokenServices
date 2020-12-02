using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;

namespace FluffyBunny4.Services
{
    public class KeyVaultTokenCreationService : DefaultTokenCreationService
    {
        private ITenantRequestContext _tenantRequestContext;
        private ITenantResolver _tenantResolver;

        public KeyVaultTokenCreationService(
            ITenantRequestContext tenantRequestContext,
            ITenantResolver tenantResolver,
            ISystemClock clock,
            IKeyMaterialService keys,
            IdentityServerOptions options,
            ILogger<DefaultTokenCreationService> logger) : base(clock, keys, options, logger)
        {
            _tenantRequestContext = tenantRequestContext;
            _tenantResolver = tenantResolver;
        }
        public override async Task<string> CreateTokenAsync(Token token)
        {
            var signatureProvider = await _tenantResolver.GetSignatureProviderAsync(_tenantRequestContext.TenantId);
            var jwtToken = await signatureProvider.CreateJwtAsync(token);
            return jwtToken;
        }
         
        
    }
}
