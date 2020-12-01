using System.Collections.Generic;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.Clients.Secrets
{
    public class IndexModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private ILogger<IndexModel> _logger;

        public IndexModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            ILogger<IndexModel> logger)
        {
            _adminServices = adminServices;
            _sessionTenantAccessor = sessionTenantAccessor;
            _logger = logger;
        }

        [BindProperty]
        public string TenantId { get; set; }
        public int ApiResourceId { get; private set; }
        

        [ViewData]
        public SecretsSortType DescriptionSortType { get; set; }
      
        public IEnumerable<ClientSecret> Entities { get; private set; }
        [ViewData]
        public SecretsSortType ExpirationSortType { get; set; }
        [ViewData]
        public SecretsSortType CurrentSortType { get; set; }


        public async Task OnGetAsync(int id, SecretsSortType sortOrder)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            ApiResourceId = id;
            switch (sortOrder)
            {

                case SecretsSortType.ExpirationAsc:
                   Entities =
                        await _adminServices.GetAllClientSecretsAsync(
                            TenantId,
                            id,
                            SecretsSortType.ExpirationAsc);
                    ExpirationSortType = SecretsSortType.ExpirationDesc;
                    break;
                case SecretsSortType.ExpirationDesc:
                    Entities =
                        await _adminServices.GetAllClientSecretsAsync(
                            TenantId,
                            id,
                            SecretsSortType.ExpirationDesc);
                    ExpirationSortType = SecretsSortType.ExpirationAsc;
                    break;
                case SecretsSortType.DescriptionDesc:
                    Entities =
                        await _adminServices.GetAllClientSecretsAsync(
                            TenantId,
                            id,
                            SecretsSortType.DescriptionDesc);
                    DescriptionSortType = SecretsSortType.DescriptionAsc;
                    ExpirationSortType = SecretsSortType.ExpirationDesc;
                    break;
                case SecretsSortType.DescriptionAsc:
                default:
                    Entities =
                        await _adminServices.GetAllClientSecretsAsync(
                            TenantId,
                            id,
                            SecretsSortType.DescriptionAsc);
                    DescriptionSortType = SecretsSortType.DescriptionDesc;
                    ExpirationSortType = SecretsSortType.ExpirationDesc;

                    break;
            }

            CurrentSortType = sortOrder;
        }
        
    }
}
