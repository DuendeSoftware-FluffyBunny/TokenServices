using FluffyBunny4.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;

namespace FluffyBunny4.Stores
{
    public interface ITenantAdmin
    {
        Task CreateTenantAsync(string tenantId);
        Task CreateTenantDatabasesAsync(string tenantId);
        Task UpsertTenantConfigAsync(TenantHandle tenantHandle);
        Task DeleteTenantAsync(string tenantId);
        Task<List<ClientExtra>> GetAllClientsAsync(string tenantId);
        Task<ClientExtra> GetClientByIdAsync(string tenantId, string clientId);
        Task DeleteClientAsync(string clientId);
        Task UpsertClientAsync(ClientExtraHandle client);
        Task AddClientSecretAsync(string tenantId, string clientId, Secret secret);
        Task<List<ApiResource>> GetAllApiResourcesAsync(string tenantId);
        Task<List<ApiResourceHandle>> GetAllApiResourceHandlesAsync(string tenantId);
        Task<ApiResource> GetApiResourceAsync(string tenantId, string apiName);
        Task<ApiResourceHandle> GetApiResourceHandleAsync(string tenantId, string apiName);
        Task DeleteApiResourceAsync(string tenantId, string apiName);
        Task UpsertApiResourceHandleAsync(string tenantId, ApiResourceHandle apiResource);
    }
}
