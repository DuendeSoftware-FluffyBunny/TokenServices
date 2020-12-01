using System.Collections.Generic;
using System.Threading.Tasks;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenant.ExternalServices
{
    public class IndexModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private IPagingHelper _pagingHelper;
        private ILogger<IndexModel> _logger;

        public IndexModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            IPagingHelper pagingHelper,
            ILogger<IndexModel> logger)
        {
            _adminServices = adminServices;
            _sessionTenantAccessor = sessionTenantAccessor;
            _pagingHelper = pagingHelper;
            _logger = logger;
        }
        [BindProperty]
        public string TenantId { get; set; }
         
        public List<SelectListItem> PageSizeOptions { get; private set; }

        [ViewData]
        public ExternalServicesSortType NameSortType { get; set; }
        public PaginatedList<IdentityServer.EntityFramework.Storage.Entities.ExternalService> PagedEntities { get; private set; }
        [ViewData]
        public ExternalServicesSortType EnabledSortType { get; set; }
        [ViewData]
        public ExternalServicesSortType CurrentSortType { get; set; }

        [ViewData]
        public int PageSize { get; set; }

        [BindProperty]
        public int SelectedPageSize { get; set; }

        public async Task OnGetAsync(ExternalServicesSortType sortOrder, int? pageNumber, int? pageSize)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            PageSize = _pagingHelper.ValidatePageSize(pageSize);
            PageSizeOptions = _pagingHelper.GetPagingSizeOptions();
            SelectedPageSize = PageSize;

            switch (sortOrder)
            {

                case ExternalServicesSortType.EnabledAsc:
                    PagedEntities =
                        await _adminServices.PageExternalServicesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1), 
                            PageSize,
                            ExternalServicesSortType.EnabledAsc);
                    EnabledSortType = ExternalServicesSortType.EnabledDesc;
                    break;
                case ExternalServicesSortType.EnabledDesc:
                    PagedEntities =
                        await _adminServices.PageExternalServicesAsync(
                            TenantId, 
                            (int)(pageNumber ?? 1), 
                            PageSize,
                            ExternalServicesSortType.EnabledDesc);
                    EnabledSortType = ExternalServicesSortType.EnabledAsc;
                    break;
                case ExternalServicesSortType.NameDesc:
                    PagedEntities =
                        await _adminServices.PageExternalServicesAsync(
                            TenantId, 
                            (int)(pageNumber ?? 1), 
                            PageSize,
                            ExternalServicesSortType.NameDesc);
                    NameSortType = ExternalServicesSortType.NameAsc;
                    EnabledSortType = ExternalServicesSortType.EnabledDesc;
                    break;
                case ExternalServicesSortType.NameAsc:
                default:
                    PagedEntities =
                        await _adminServices.PageExternalServicesAsync(
                            TenantId, 
                            (int)(pageNumber ?? 1), 
                            PageSize,
                            ExternalServicesSortType.NameAsc);
                    NameSortType = ExternalServicesSortType.NameDesc;
                    EnabledSortType = ExternalServicesSortType.EnabledDesc;

                    break;
            }

            CurrentSortType = sortOrder;

        }
        public async Task<IActionResult> OnPostPageSizeAsync()
        {
            return RedirectToPage(new { sortOrder = CurrentSortType, pageNumber = 1, pageSize = SelectedPageSize });
        }
    }
}
