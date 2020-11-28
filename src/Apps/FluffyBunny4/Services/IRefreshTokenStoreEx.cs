using System.Threading.Tasks;
using Duende.IdentityServer.Stores;

namespace FluffyBunny4.Services
{
    public interface IRefreshTokenStoreEx: IRefreshTokenStore 
    {
        Task RemoveSubjectTokensAsync(string subjectId);
    }
}
