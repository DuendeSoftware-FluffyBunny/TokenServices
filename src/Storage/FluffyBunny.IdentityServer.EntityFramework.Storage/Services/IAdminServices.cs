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

    public enum ExternalServiceSortType
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

        Task UpsertExternalServiceAsync(string tenantName,ExternalService entity);
        Task<ExternalService> GetExternalServiceByNameAsync(string tenantName, string name);
        Task<ExternalService> GetExternalServiceByIdAsync(string tenantName, int id);
        Task DeleteExternalServiceByNameAsync(string tenantName, string name);
        Task DeleteExternalServiceByIdAsync(string tenantName, int id);
        Task<PaginatedList<ExternalService>> PageExternalServicesAsync(string tenantName, int pageNumber, int pageSize, ExternalServiceSortType sortType);
    }
}
