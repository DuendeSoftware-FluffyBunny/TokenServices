using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Entities;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Services
{
    public enum TenantSortType
    {
        NameDesc,
        NameAsc,
        EnabledDesc,
        EnabledAsc
    }
    public interface IAdminServices
    {
        Task CreateTenantAsync(string name);
        Task<IEnumerable<Tenant>> GetAllTenantsAsync();
        Task<PaginatedList<Tenant>> PageTenantsAsync(int pageNumber, int pageSize, TenantSortType sortType);
        Task<Tenant> GetTenantByNameAsync(string tenantId);
        Task UpdateTenantAsync(Tenant tenant);
    }
}
