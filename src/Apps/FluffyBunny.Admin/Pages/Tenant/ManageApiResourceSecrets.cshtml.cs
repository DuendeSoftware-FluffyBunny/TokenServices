using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenant
{
    public class ManageApiResourceSecretsModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private ILogger<ManageApiResourcesModel> _logger;

        public ManageApiResourceSecretsModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            ILogger<ManageApiResourcesModel> logger)
        {
            _adminServices = adminServices;
            _sessionTenantAccessor = sessionTenantAccessor;
            _logger = logger;
        }

        [BindProperty]
        public string TenantId { get; set; }
        public int ApiResourceId { get; private set; }
        

        [ViewData]
        public ApiResourceSecretsSortType DescriptionSortType { get; set; }
      
        public IEnumerable<ApiResourceSecret> Entities { get; private set; }
        [ViewData]
        public ApiResourceSecretsSortType ExpirationSortType { get; set; }
        [ViewData]
        public ApiResourceSecretsSortType CurrentSortType { get; set; }


        public async Task OnGetAsync(int id, ApiResourceSecretsSortType sortOrder)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            ApiResourceId = id;
            switch (sortOrder)
            {

                case ApiResourceSecretsSortType.ExpirationAsc:
                   Entities =
                        await _adminServices.GetAllApiResourceSecretsAsync(
                            TenantId,
                            id,
                            ApiResourceSecretsSortType.ExpirationAsc);
                    ExpirationSortType = ApiResourceSecretsSortType.ExpirationDesc;
                    break;
                case ApiResourceSecretsSortType.ExpirationDesc:
                    Entities =
                        await _adminServices.GetAllApiResourceSecretsAsync(
                            TenantId,
                            id,
                            ApiResourceSecretsSortType.ExpirationDesc);
                    ExpirationSortType = ApiResourceSecretsSortType.ExpirationAsc;
                    break;
                case ApiResourceSecretsSortType.DescriptionDesc:
                    Entities =
                        await _adminServices.GetAllApiResourceSecretsAsync(
                            TenantId,
                            id,
                            ApiResourceSecretsSortType.DescriptionDesc);
                    DescriptionSortType = ApiResourceSecretsSortType.DescriptionAsc;
                    ExpirationSortType = ApiResourceSecretsSortType.ExpirationDesc;
                    break;
                case ApiResourceSecretsSortType.DescriptionAsc:
                default:
                    Entities =
                        await _adminServices.GetAllApiResourceSecretsAsync(
                            TenantId,
                            id,
                            ApiResourceSecretsSortType.DescriptionAsc);
                    DescriptionSortType = ApiResourceSecretsSortType.DescriptionDesc;
                    ExpirationSortType = ApiResourceSecretsSortType.ExpirationDesc;

                    break;
            }

            CurrentSortType = sortOrder;
        }
        
    }
}
