using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Mvc;
using OIDCPipeline.Core.Extensions;
using OIDCPipeline.Core.Validation.Models;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    internal class OIDCResponseGenerator : IOIDCResponseGenerator
    {
        private IOIDCPipelineStore _oidcPipelineStore;

        public OIDCResponseGenerator(IOIDCPipelineStore oidcPipelineStore)
        {
            _oidcPipelineStore = oidcPipelineStore;
        }
        public async Task<IActionResult> CreateAuthorizeResponseActionResultAsync(
            string key,
            bool delete = true)
        {
            
       
            var original = await _oidcPipelineStore.GetOriginalIdTokenRequestAsync(key);
            var downstream = await _oidcPipelineStore.GetDownstreamIdTokenResponseAsync(key);
                    
            if (!string.IsNullOrWhiteSpace(original.Nonce))
            {
                downstream.Custom["nonce"] = original.Nonce;
            }

            var header = new JwtHeader();
            var handler = new JwtSecurityTokenHandler();
            var idToken = handler.ReadJwtToken(downstream.IdToken);
            var claims = idToken.Claims.ToList();
            var scope = (from item in claims where item.Type == "scope" select item).FirstOrDefault();

            var authResponse = new AuthorizeResponse();
            
            if ( original.CodeChallenge.IsPresent() &&
                 original.CodeChallengeMethod.IsPresent())
            {
                // slide out the stored stuff , as we will have a incoming token request with a code
                await _oidcPipelineStore.StoreDownstreamIdTokenResponseAsync(key, downstream);
                authResponse.Code = key;
            }
            else
            {
                authResponse.IdentityToken = downstream.IdToken;
                authResponse.AccessToken = downstream.AccessToken;
            }
            authResponse.Request = original;
            authResponse.Downstream = downstream;
            authResponse.Scope = scope?.Value;

             
           
            var authorizeResult = new AuthorizeResult(authResponse);

            if (delete && authResponse.Code.IsMissing())
            {
                await _oidcPipelineStore.DeleteStoredCacheAsync(key);
            }
            return authorizeResult;
        }

        
    }
}
