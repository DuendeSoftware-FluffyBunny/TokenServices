﻿using System;
using System.Collections.Generic;
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
    public enum ApiResourceSecretsSortType
    {
        DescriptionDesc,
        DescriptionAsc,
        ExpirationDesc,
        ExpirationAsc
    }
    public interface IAdminServices
    {
        #region Tenant
        Task CreateTenantAsync(string name);
        Task<IEnumerable<Tenant>> GetAllTenantsAsync();
        Task<PaginatedList<Tenant>> PageTenantsAsync(int pageNumber, int pageSize, TenantsSortType sortType);
        Task<Tenant> GetTenantByNameAsync(string tenantId);
        Task UpdateTenantAsync(Tenant tenant);

        #endregion
        #region ExternalServices

        Task UpsertExternalServiceAsync(string tenantName, ExternalService entity);
        Task<ExternalService> GetExternalServiceByNameAsync(string tenantName, string name);
        Task<ExternalService> GetExternalServiceByIdAsync(string tenantName, int id);
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

        #endregion

        Task UpsertApiResourceSecretAsync(string tenantName, int apiResourceId, ApiResourceSecret entity);
        Task<ApiResourceSecret> GetApiResourceSecretByIdAsync(string tenantName, int apiResourceId, int id);
        Task DeleteApiResourceBySecretIdAsync(string tenantName, int apiResourceId, int id);

        Task<IEnumerable<ApiResourceSecret>> GetAllApiResourceSecretsAsync(string tenantName, int apiResourceId, ApiResourceSecretsSortType sortType);
    }
}
