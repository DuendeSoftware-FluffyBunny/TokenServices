using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using FluffyBunny4.DotNetCore.Services;
using FluffyBunny4.Models;

namespace FluffyBunny4.Services
{
    public class KeyVaultTokenCreationService : DefaultTokenCreationService
    {
        private IScopedContext<TenantContext> _scopedTenantContext;
        private ITenantResolver _tenantResolver;

        public KeyVaultTokenCreationService(
            IScopedContext<TenantContext> scopedTenantContext,
            ITenantResolver tenantResolver,
            ISystemClock clock,
            IKeyMaterialService keys,
            IdentityServerOptions options,
            ILogger<DefaultTokenCreationService> logger) : base(clock, keys, options, logger)
        {
            _scopedTenantContext = scopedTenantContext;
            _tenantResolver = tenantResolver;
        }
        public override async Task<string> CreateTokenAsync(Token token)
        {
            var signatureProvider = await _tenantResolver.GetSignatureProviderAsync(_scopedTenantContext.Context.TenantName);
            var jwtToken = await signatureProvider.CreateJwtAsync(token);
            return jwtToken;
        }
         
        
    }
}
