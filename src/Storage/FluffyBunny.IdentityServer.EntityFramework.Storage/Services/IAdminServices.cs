using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Duende.IdentityServer.EntityFramework.Entities;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Entities;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Services
{
    public enum TenantsSortType
    {
        NameDesc,
        NameAsc,
        EnabledDesc,
        EnabledAsc
    }
    public enum ClientsSortType
    {
        NameDesc,
        NameAsc,
        EnabledDesc,
        EnabledAsc
    }
    public enum ExternalServicesSortType
    {
        NameDesc,
        NameAsc,
        EnabledDesc,
        EnabledAsc
    }
    public enum ApiResourcesSortType
    {
        NameDesc,
        NameAsc,
        EnabledDesc,
        EnabledAsc
    }
    public enum GrantTypesSortType
    {
        NameDesc,
        NameAsc 
    }
    public enum ClientScopesSortType
    {
        NameDesc,
        NameAsc
    }
    public enum ApiResourceScopesSortType
    {
        NameDesc,
        NameAsc
    }
    public enum SecretsSortType
    {
        DescriptionDesc,
        DescriptionAsc,
        ExpirationDesc,
        ExpirationAsc
    }
    public interface IAdminServices
    {
        #region Tenant

        Task EnsureMainConfigurationDatabaseAsync();
        Task CreateTenantAsync(string name);
        Task EnsureTenantDatabaseAsync(string name);
        Task<IEnumerable<Tenant>> GetAllTenantsAsync();
        Task<PaginatedList<Tenant>> PageTenantsAsync(int pageNumber, int pageSize, TenantsSortType sortType);
        Task<Tenant> GetTenantByNameAsync(string tenantId);
    
        Task UpdateTenantAsync(Tenant tenant);

        #endregion
        #region ExternalServices

        Task UpsertExternalServiceAsync(string tenantName, ExternalService entity);
        Task<ExternalService> GetExternalServiceByNameAsync(string tenantName, string name);
        Task<ExternalService> GetExternalServiceByIdAsync(string tenantName, int id);
        Task<List<ExternalService>> GetAllExternalServicesAsync(string tenantName);
        Task DeleteExternalServiceByNameAsync(string tenantName, string name);
        Task DeleteExternalServiceByIdAsync(string tenantName, int id);
        Task<PaginatedList<ExternalService>> PageExternalServicesAsync(string tenantName, int pageNumber, int pageSize, ExternalServicesSortType sortType);

        #endregion

        #region ApiResources
        Task UpsertApiResourceAsync(string tenantName, ApiResource entity);
        Task<ApiResource> GetApiResourceByNameAsync(string tenantName, string name);
        Task<ApiResource> GetApiResourceByIdAsync(string tenantName, int id);
        Task DeleteApiResourceByNameAsync(string tenantName, string name);
        Task DeleteApiResourceByIdAsync(string tenantName, int id);
        Task<PaginatedList<ApiResource>> PageApiResourcesAsync(string tenantName, int pageNumber, int pageSize, ApiResourcesSortType sortType);
        Task<List<ApiResource>> GetAllApiResourcesAsync(string tenantName);
        Task<List<ApiResourceScope>> GetAllApiResourceScopesAsync(string tenantName, ClientScopesSortType sortType);
        #endregion

        #region ApiResourceSecrets

        Task UpsertApiResourceSecretAsync(string tenantName, int apiResourceId, ApiResourceSecret entity);

        Task<ApiResourceSecret> GetApiResourceSecretByIdAsync(string tenantName, int apiResourceId, int id);
        Task DeleteApiResourceBySecretIdAsync(string tenantName, int apiResourceId, int id);
        Task<IEnumerable<ApiResourceSecret>> GetAllApiResourceSecretsAsync(string tenantName, int apiResourceId, SecretsSortType sortType);

        #endregion
        #region ApiResourceScopes
        Task UpsertApiResourceScopeAsync(string tenantName, int apiResourceId, ApiResourceScope entity);

        Task<ApiResourceScope> GetApiResourceScopeByIdAsync(string tenantName, int apiResourceId, int id);
        Task<ApiResourceScope> GetApiResourceScopeByNameAsync(string tenantName, int apiResourceId, string name);
        Task DeleteApiResourceByScopeIdAsync(string tenantName, int apiResourceId, int id);
        Task<IEnumerable<ApiResourceScope>> GetAllApiResourceScopesAsync(string tenantName, int apiResourceId, ApiResourceScopesSortType sortType = ApiResourceScopesSortType.NameDesc);
        #endregion
        #region Clients
        Task UpsertClientAsync(string tenantName, ClientExtra entity);
 
        Task<ClientExtra> GetClientByIdAsync(string tenantName, int id);
        Task<ClientExtra> GetClientByClientIdAsync(string tenantName, string clientId);

        Task DeleteClientByNameAsync(string tenantName, string name);
        Task DeleteClientByIdAsync(string tenantName, int id);
        Task<PaginatedList<ClientExtra>> PageClientsAsync(string tenantName, int pageNumber, int pageSize, ClientsSortType sortType);
        #endregion

        #region ClientSecret

        Task UpsertClientSecretAsync(string tenantName, int clientId, ClientSecret entity);
        Task<ClientSecret> GetClientSecretByIdAsync(string tenantName, int clientId, int id);
        Task DeleteClientSecretByIdAsync(string tenantName, int clientId, int id);
        Task<IEnumerable<ClientSecret>> GetAllClientSecretsAsync(string tenantName, int id, SecretsSortType sortType);

        #endregion

        #region ClientAllowedGrants
        Task UpsertClientAllowedGrantTypesAsync(string tenantName, int clientId, ClientGrantType entity);
        Task<ClientGrantType> GetClientGrantTypeByIdAsync(string tenantName, int clientId, int id);
        Task DeleteClientGrantTypByIdAsync(string tenantName, int clientId, int id);
        Task<IEnumerable<ClientGrantType>> GetAllClientAllowedGrantTypesAsync(string tenantName, int id, GrantTypesSortType sortType);


        #endregion

    }
}
