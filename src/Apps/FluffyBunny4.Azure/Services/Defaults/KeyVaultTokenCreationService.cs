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
        private IScopedContext<TenantRequestContext> _scopedTenantRequestContext;
        private ITenantResolver _tenantResolver;

        public KeyVaultTokenCreationService(
            IScopedContext<TenantRequestContext> scopedTenantRequestContext,
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
            var signatureProvider = await _tenantResolver.GetSignatureProviderAsync(_scopedTenantRequestContext.Context.TenantName);
            var jwtToken = await signatureProvider.CreateJwtAsync(token);
            return jwtToken;
        }
         
        
    }
}
