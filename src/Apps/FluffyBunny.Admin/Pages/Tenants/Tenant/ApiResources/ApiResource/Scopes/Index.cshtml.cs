using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.ApiResources.ApiResource.Scopes
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
        public ApiResourceScopesSortType NameSortType { get; set; }

        public IEnumerable<ApiResourceScope> Entities { get; private set; }
        [ViewData] 
        public ApiResourceScopesSortType CurrentSortType { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, ApiResourceScopesSortType sortOrder)
        {
            try
            {
                TenantId = _sessionTenantAccessor.TenantId;
                ApiResourceId = id;
                switch (sortOrder)
                {

                    case ApiResourceScopesSortType.NameAsc:
                        Entities =
                             await _adminServices.GetAllApiResourceScopesAsync(
                                 TenantId,
                                 id,
                                 ApiResourceScopesSortType.NameAsc);
                        NameSortType = ApiResourceScopesSortType.NameDesc;
                        break;
                    case ApiResourceScopesSortType.NameDesc:
                        Entities =
                            await _adminServices.GetAllApiResourceScopesAsync(
                                TenantId,
                                id,
                                ApiResourceScopesSortType.NameDesc);
                        NameSortType = ApiResourceScopesSortType.NameAsc;
                        break;
                   
                }

                CurrentSortType = sortOrder;
                return Page();

            }
            catch (Exception ex)
            {
                return RedirectToPage("../Index", new { id = ApiResourceId });
            }
        }
    }
}
