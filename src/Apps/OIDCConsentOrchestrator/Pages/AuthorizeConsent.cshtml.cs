using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using FluffyBunny4.DotNetCore.Extensions;
using FluffyBunny4.DotNetCore.Services;
using IdentityModel;
using IdentityModel.Client;
using IdentityModel.FluffyBunny4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OIDCConsentOrchestrator.Linq;
using OIDCConsentOrchestrator.Models;
using OIDCConsentOrchestrator.Models.Client;
using OIDCConsentOrchestrator.Services;
using OIDCPipeline.Core;
using OIDCPipeline.Core.Validation.Models;

namespace OIDCConsentOrchestrator.Pages
{
    [Authorize]
    public class AuthorizeConsentModel : PageModel
    {
        private const string ScopeBaseUrl = "https://www.companyapis.com/auth/";
        public string IdToken { get; set; }
        private IOIDCPipeLineKey _oidcPipelineKey;
        private IOIDCPipelineStore _oidcPipelineStore;
        private IFluffyBunnyTokenService _fluffyBunnyTokenService;
        private FluffyBunny4TokenServiceConfiguration _FluffyBunny4TokenServiceConfiguration;
        private ITokenServiceDiscoveryCache _tokenServiceDiscoveryCache;
        private ISerializer _serializer;
        private ILogger<AuthorizeConsentModel> _logger;
      

        public AuthorizeConsentModel(
            IOIDCPipeLineKey oidcPipelineKey,
            IOIDCPipelineStore oidcPipelineStore,
            IFluffyBunnyTokenService fluffyBunnyTokenService,
            IOptions<FluffyBunny4TokenServiceConfiguration> optionsFluffyBunny4TokenServiceConfiguration,
            ITokenServiceDiscoveryCache tokenServiceDiscoveryCache,
            ISerializer serializer,
            ILogger<AuthorizeConsentModel> logger)
        {
            _oidcPipelineKey = oidcPipelineKey;
            _oidcPipelineStore = oidcPipelineStore;
            _fluffyBunnyTokenService = fluffyBunnyTokenService;
            _FluffyBunny4TokenServiceConfiguration = optionsFluffyBunny4TokenServiceConfiguration.Value;
            _tokenServiceDiscoveryCache = tokenServiceDiscoveryCache;
            _serializer = serializer;
            _logger = logger;
        }

        public ValidatedAuthorizeRequest OriginalAuthorizationRequest { get; private set; }
        public List<ConsentResponseContainer> ConsentResponseContainers { get; private set; }
      
        public string NameIdentifier { get; private set; }
        public string[] Scopes { get; private set; }
        public ArbitraryTokenTokenRequestV2 ArbitraryTokenTokenRequestV2 { get; private set; }
        public TokenExchangeTokenRequest TokenExchangeTokenRequest { get; private set; }
        public string JsonArbitraryTokenTokenRequestV2 { get; private set; }
        public TokenResponse TokenPayload { get; private set; }

        public class CustomPayloadContainer
        {
            public string Name { get; set; }
            public object CustomPayload { get; set; }
        }
        public class ConsentResponseContainer
        {
            public ConsentDiscoveryDocumentResponse DiscoveryDocument { get; set; }
            public ConsentAuthorizeResponse Response { get; set; }
            public ConsentAuthorizeRequest Request { get; set; }
        }
        public async Task OnGetAsync()
        {
            NameIdentifier = User.Claims.GetClaimsByType(".externalNamedIdentitier").FirstOrDefault().Value;
            IdToken = User.Claims.GetClaimsByType(".id_token").FirstOrDefault().Value;
         
            var key = _oidcPipelineKey.GetOIDCPipeLineKey();
            OriginalAuthorizationRequest = await _oidcPipelineStore.GetOriginalIdTokenRequestAsync(key);

            var queryScopes = (from item in OriginalAuthorizationRequest.Raw
                               where item.Key == "scope"
                               let scopes = item.Value.Split(" ")
                               from cItem in scopes
                               where cItem.StartsWith(ScopeBaseUrl)
                               select cItem).ToList();

            var scope = string.Join(" ", queryScopes);
            scope += " offline_access";

            
            

            var docoTokenService = await _tokenServiceDiscoveryCache.GetAsync();
            /*
            ArbitraryTokenTokenRequestV2 = new ArbitraryTokenTokenRequestV2() {
                Address = docoTokenService.TokenEndpoint,
                ClientId = _FluffyBunny4TokenServiceConfiguration.ClientId,
                ClientSecret = _FluffyBunny4TokenServiceConfiguration.ClientSecret,
                Subject = NameIdentifier,
                Scope = new HashSet<string>(),
                ArbitraryClaims = new Dictionary<string, List<string>>(),
                ArbitraryAmrs = new List<string>(),
                ArbitraryAudiences = new List<string>(),
                CustomPayload = null
            };
            */
            TokenExchangeTokenRequest = new TokenExchangeTokenRequest
            {
                Address = docoTokenService.TokenEndpoint,
                ClientId = _FluffyBunny4TokenServiceConfiguration.ClientId,
                ClientSecret = _FluffyBunny4TokenServiceConfiguration.ClientSecret,
                Scope = scope,
                GrantType = OidcConstants.GrantTypes.TokenExchange,
                SubjectTokenType = FluffyBunny4.Constants.TokenExchangeTypes.IdToken,
                SubjectToken = IdToken
            };
 

            var httpClient = new HttpClient();
            TokenPayload = await _fluffyBunnyTokenService.RequestTokenExchangeTokenAsync(httpClient, TokenExchangeTokenRequest);
            var tokenExchangePayload = new
            {
                access_token = TokenPayload.AccessToken,
                refresh_token = TokenPayload.RefreshToken,
                scope = TokenPayload.Scope,
                token_type = TokenPayload.TokenType
            };
            await _oidcPipelineStore.StoreTempCustomObjectAsync(key, "token-exchange", tokenExchangePayload);
        }

     
    }
}
