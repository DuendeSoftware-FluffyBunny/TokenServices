using System.Threading.Tasks;
using IdentityModel.OidcClient.Results;

namespace FluffyBunny4.Services
{
    public interface IIdentityTokenValidator
    {
        Task<IdentityTokenValidationResult> ValidateIdTokenAsync(string identityToken, string issuerKey);
    }
}