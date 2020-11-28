using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
    public interface ISigninManager
    {
        Task SignOutAsync();
    }
}
