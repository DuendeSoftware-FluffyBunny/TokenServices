using FluffyBunny4.Models;
using System.Threading.Tasks;

namespace FluffyBunny4.Services
{

    public interface IClientExtraAdminStore 
    {
        Task<ClientExtraResponse> UpsertItemAsync(ClientExtra item);
        Task<ClientExtraResponse> DeleteItemAsync(string id);
    }
}
