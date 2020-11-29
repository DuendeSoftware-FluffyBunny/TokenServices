using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using FluffyBunny4.DotNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private ILogger<AddTenantModel> _logger;

        public string TenantId { get; private set; }
        public int PageIndex { get; set; }
        public class InputModel
        {
            [Required]
            public string Name { get; set; }
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public PaginatedList<IdentityServer.EntityFramework.Storage.Entities.Tenant> PagedTenants { get; private set; }
        public IEnumerable<FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.Tenant> Tenants { get; private set; }

        [ViewData]
        public TenantSortType NameSortType { get; set; }
        [ViewData]
        public TenantSortType EnabledSortType { get; set; }
        [ViewData]
        public TenantSortType CurrentSortType { get; set; }

        [ViewData]
        public int PageSize { get; set; }

        [BindProperty]
        public int SelectedPageSize { get; set; }
        public List<SelectListItem> PageSizeOptions { get; set; }

        public IndexModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            ILogger<AddTenantModel> logger)
        {
            _adminServices = adminServices;
            _sessionTenantAccessor = sessionTenantAccessor;
            _logger = logger;
        }
        public async Task OnGetAsync(TenantSortType sortOrder,int? pageNumber,int? pageSize)
        {
            PageSize = (int)(pageSize ?? 4);
            PageSizeOptions = new List<SelectListItem>()
            {
                new SelectListItem("4", "4",PageSize==4),
                new SelectListItem("8", "8",PageSize==8),
                new SelectListItem("16", "16",PageSize==16),
                new SelectListItem("32", "32",PageSize==32),
            };
            SelectedPageSize = PageSize;



            switch (sortOrder)
            {
                
                case TenantSortType.EnabledAsc:
                    PagedTenants =
                        await _adminServices.PageTenantsAsync((int)(pageNumber ?? 1), PageSize,
                            TenantSortType.EnabledAsc);
                    EnabledSortType = TenantSortType.EnabledDesc;
                    break;
                case TenantSortType.EnabledDesc:
                    PagedTenants =
                        await _adminServices.PageTenantsAsync((int)(pageNumber ?? 1), PageSize,
                            TenantSortType.EnabledDesc);
                    EnabledSortType = TenantSortType.EnabledAsc;
                    break;
                case TenantSortType.NameDesc:
                    PagedTenants =
                        await _adminServices.PageTenantsAsync((int)(pageNumber ?? 1), PageSize,
                            TenantSortType.NameDesc);
                    NameSortType = TenantSortType.NameAsc;
                    EnabledSortType = TenantSortType.EnabledDesc; 
                    break;
                case TenantSortType.NameAsc:
                default:
                    PagedTenants =
                        await _adminServices.PageTenantsAsync((int)(pageNumber ?? 1), PageSize,
                            TenantSortType.NameAsc);
                    NameSortType = TenantSortType.NameDesc;
                    EnabledSortType = TenantSortType.EnabledDesc;

                    break;
            }

            CurrentSortType = sortOrder;
            TenantId = _sessionTenantAccessor.TenantId;
           
        }
        public async Task<IActionResult> OnPostSwitchAsync(string tenantName)
        {
            _sessionTenantAccessor.TenantId = tenantName;
            return RedirectToPage("/Tenant/Index");
        }
        public async Task<IActionResult> OnPostPageSizeAsync( )
        {
            return RedirectToPage(new {sortOrder = CurrentSortType, pageNumber = 1, pageSize = SelectedPageSize});
        }
    }
}
