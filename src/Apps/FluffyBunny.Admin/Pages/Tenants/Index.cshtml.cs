using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using FluffyBunny4.DotNetCore;
using FluffyBunny4.DotNetCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants
{
    [Authorize]
    public class IndexModel : PageModel
    {
        
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private IPagingHelper _pagingHelper;
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
        public PaginatedList<IdentityServer.EntityFramework.Storage.Entities.Tenant> PagedEntities { get; private set; }
        public IEnumerable<FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.Tenant> Tenants { get; private set; }

        [ViewData]
        public TenantsSortType NameSortType { get; set; }
        [ViewData]
        public TenantsSortType EnabledSortType { get; set; }
        [ViewData]
        public TenantsSortType CurrentSortType { get; set; }

        [ViewData]
        public int PageSize { get; set; }

        [BindProperty]
        public int SelectedPageSize { get; set; }
        public List<SelectListItem> PageSizeOptions { get; set; }

        public IndexModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            IPagingHelper pagingHelper,
            ILogger<AddTenantModel> logger)
        {
            _adminServices = adminServices;
            _sessionTenantAccessor = sessionTenantAccessor;
            _pagingHelper = pagingHelper;
            _logger = logger;
        }
        public async Task OnGetAsync(TenantsSortType sortOrder,int? pageNumber,int? pageSize)
        {
            PageSize = _pagingHelper.ValidatePageSize(pageSize);
            PageSizeOptions = _pagingHelper.GetPagingSizeOptions();
            SelectedPageSize = PageSize;


            switch (sortOrder)
            {
                
                case TenantsSortType.EnabledAsc:
                    PagedEntities =
                        await _adminServices.PageTenantsAsync((int)(pageNumber ?? 1), PageSize,
                            TenantsSortType.EnabledAsc);
                    EnabledSortType = TenantsSortType.EnabledDesc;
                    break;
                case TenantsSortType.EnabledDesc:
                    PagedEntities =
                        await _adminServices.PageTenantsAsync((int)(pageNumber ?? 1), PageSize,
                            TenantsSortType.EnabledDesc);
                    EnabledSortType = TenantsSortType.EnabledAsc;
                    break;
                case TenantsSortType.NameDesc:
                    PagedEntities =
                        await _adminServices.PageTenantsAsync((int)(pageNumber ?? 1), PageSize,
                            TenantsSortType.NameDesc);
                    NameSortType = TenantsSortType.NameAsc;
                    EnabledSortType = TenantsSortType.EnabledDesc; 
                    break;
                case TenantsSortType.NameAsc:
                default:
                    PagedEntities =
                        await _adminServices.PageTenantsAsync((int)(pageNumber ?? 1), PageSize,
                            TenantsSortType.NameAsc);
                    NameSortType = TenantsSortType.NameDesc;
                    EnabledSortType = TenantsSortType.EnabledDesc;

                    break;
            }

            CurrentSortType = sortOrder;
            TenantId = _sessionTenantAccessor.TenantId;
           
        }
        public async Task<IActionResult> OnPostSwitchAsync(string tenantName)
        {
            _sessionTenantAccessor.TenantId = tenantName;
            return RedirectToPage("/Tenants/Tenant/Index");
        }
        public async Task<IActionResult> OnPostPageSizeAsync( )
        {
            return RedirectToPage(new {sortOrder = CurrentSortType, pageNumber = 1, pageSize = SelectedPageSize});
        }
    }
}
