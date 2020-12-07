using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer.ResponseHandling;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace FluffyBunny4.Stores
{
    public class MyTokenResponseGenerator: TokenResponseGenerator
    {
        public MyTokenResponseGenerator(
            ISystemClock clock, 
            ITokenService tokenService, 
            IRefreshTokenService refreshTokenService, 
            IScopeParser scopeParser, 
            IResourceStore resources, 
            IClientStore clients, 
            ILogger<TokenResponseGenerator> logger) : base(clock, tokenService, refreshTokenService, scopeParser, resources, clients, logger)
        {
        }

        protected async override Task<(string accessToken, string refreshToken)> CreateAccessTokenAsync(ValidatedTokenRequest request)
        {
            (string accessToken, string refreshToken) =  await base.CreateAccessTokenAsync(request);

            
            
            
            return (accessToken, refreshToken);
        }
    }
}
