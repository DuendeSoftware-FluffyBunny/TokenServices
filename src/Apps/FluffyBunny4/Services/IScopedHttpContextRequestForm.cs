using System.Collections.Specialized;
using System.Threading.Tasks;

namespace FluffyBunny4.Services
{
    public interface IScopedHttpContextRequestForm
    {
        Task<NameValueCollection> GetFormCollectionAsync();
    }
}
