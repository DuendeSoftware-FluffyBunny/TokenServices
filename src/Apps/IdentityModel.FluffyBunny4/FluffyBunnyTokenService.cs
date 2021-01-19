using FluffyBunny4.DotNetCore.Services;
using IdentityModel.Client;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityModel.FluffyBunny4
{
    public class FluffyBunnyTokenService : IFluffyBunnyTokenService
    {
        private ISerializer _serializer;

        public FluffyBunnyTokenService(ISerializer serializer)
        {
            _serializer = serializer;
        }
        public async Task<TokenResponse> RequestArbitraryTokenAsync(HttpClient httpClient, ArbitraryTokenTokenRequestV2 request)
        {
            var arbitraryTokenTokenRequest = new ArbitraryTokenTokenRequest
            {
                Address = request.Address,
                Subject = request.Subject,
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                AccessTokenLifetime = request.AccessTokenLifetime
            };

           
            if(request.CustomPayload != null )
            {
                // TODO custom payload needs to be an array.
                arbitraryTokenTokenRequest.CustomPayload = _serializer.Serialize(request.CustomPayload);
            }
            if (request.Scope != null)
            {
                arbitraryTokenTokenRequest.Scope = string.Join(" ", request.Scope);
            }
            if (request.ArbitraryAudiences != null)
            {
                arbitraryTokenTokenRequest.ArbitraryAudiences = _serializer.Serialize(request.ArbitraryAudiences);
            }
            if (request.ArbitraryAmrs != null)
            {
                arbitraryTokenTokenRequest.ArbitraryAmrs = _serializer.Serialize(request.ArbitraryAmrs);
            }
            if (request.ArbitraryClaims != null)
            {
                arbitraryTokenTokenRequest.ArbitraryClaims = _serializer.Serialize(request.ArbitraryClaims);
            }
           

            var result = await httpClient.RequestArbitraryTokenTokenAsync(arbitraryTokenTokenRequest);
            return result;
        }

        public async Task<TokenResponse> RequestArbitraryTokenAsync(HttpClient httpClient, ArbitraryTokenTokenRequest request)
        {
            var result = await httpClient.RequestArbitraryTokenTokenAsync(request);
            return result;
        }

        public async Task<TokenResponse> RequestTokenExchangeTokenAsync(HttpClient httpClient, TokenExchangeTokenRequest request)
        {

            var result = await httpClient.RequestTokenExchangeTokenAsync(request);
            return result;
           
        }
    }
}
