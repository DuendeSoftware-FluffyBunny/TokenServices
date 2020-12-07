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
        private IScopedTenantRequestContext _scopedTenantRequestContext;
        private ITenantResolver _tenantResolver;

        public KeyVaultTokenCreationService(
            IScopedTenantRequestContext scopedTenantRequestContext,
            ITenantResolver tenantResolver,
            ISystemClock clock,
            IKeyMaterialService keys,
            IdentityServerOptions options,
            ILogger<DefaultTokenCreationService> logger) : base(clock, keys, options, logger)
        {
            _scopedTenantRequestContext = scopedTenantRequestContext;
            _tenantResolver = tenantResolver;
        }
        public override async Task<string> CreateTokenAsync(Token token)
        {
            var signatureProvider = await _tenantResolver.GetSignatureProviderAsync(_scopedTenantRequestContext.TenantId);
            var jwtToken = await signatureProvider.CreateJwtAsync(token);
            return jwtToken;
        }
         
        
    }
}
