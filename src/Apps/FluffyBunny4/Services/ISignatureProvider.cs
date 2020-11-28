using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;

namespace FluffyBunny4.Services
{
    public interface ISignatureProvider
    {
        Task<string> CreateJwtAsync(JwtSecurityToken token);
        Task<string> CreateJwtAsync(Token token);
 
    }
}
