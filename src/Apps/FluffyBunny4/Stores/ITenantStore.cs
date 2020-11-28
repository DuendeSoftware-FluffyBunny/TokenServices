using FluffyBunny4.Models;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluffyBunny4.Stores
{

    public interface ITenantStore
    {
      
        Task<TenantHandle> FindTenantByIdAsync(string tenantId);

    }
}
