using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IdentityModel.FluffyBunny4
{
    public interface IFluffyBunnyTokenService
    {
        Task<TokenResponse> RequestArbitraryTokenAsync(HttpClient httpClient, ArbitraryTokenTokenRequestV2 request);
        Task<TokenResponse> RequestArbitraryTokenAsync(HttpClient httpClient, ArbitraryTokenTokenRequest request);
        Task<TokenResponse> RequestTokenExchangeTokenAsync(HttpClient httpClient, TokenExchangeTokenRequest request);

    }

    
}
