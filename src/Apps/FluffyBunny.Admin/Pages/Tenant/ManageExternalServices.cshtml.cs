using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenant
{
    public class ManageExternalServicesModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private IPagingHelper _pagingHelper;
        private ILogger<ManageExternalServicesModel> _logger;

        public ManageExternalServicesModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            IPagingHelper pagingHelper,
            ILogger<ManageExternalServicesModel> logger)
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
        public ExternalServiceSortType NameSortType { get; set; }
        public PaginatedList<IdentityServer.EntityFramework.Storage.Entities.ExternalService> PagedEntities { get; private set; }
        [ViewData]
        public ExternalServiceSortType EnabledSortType { get; set; }
        [ViewData]
        public ExternalServiceSortType CurrentSortType { get; set; }

        [ViewData]
        public int PageSize { get; set; }

        [BindProperty]
        public int SelectedPageSize { get; set; }

        public async Task OnGetAsync(ExternalServiceSortType sortOrder, int? pageNumber, int? pageSize)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            PageSize = _pagingHelper.ValidatePageSize(pageSize);
            PageSizeOptions = _pagingHelper.GetPagingSizeOptions();
            SelectedPageSize = PageSize;

            switch (sortOrder)
            {

                case ExternalServiceSortType.EnabledAsc:
                    PagedEntities =
                        await _adminServices.PageExternalServicesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1), 
                            PageSize,
                            ExternalServiceSortType.EnabledAsc);
                    EnabledSortType = ExternalServiceSortType.EnabledDesc;
                    break;
                case ExternalServiceSortType.EnabledDesc:
                    PagedEntities =
                        await _adminServices.PageExternalServicesAsync(
                            TenantId, 
                            (int)(pageNumber ?? 1), 
                            PageSize,
                            ExternalServiceSortType.EnabledDesc);
                    EnabledSortType = ExternalServiceSortType.EnabledAsc;
                    break;
                case ExternalServiceSortType.NameDesc:
                    PagedEntities =
                        await _adminServices.PageExternalServicesAsync(
                            TenantId, 
                            (int)(pageNumber ?? 1), 
                            PageSize,
                            ExternalServiceSortType.NameDesc);
                    NameSortType = ExternalServiceSortType.NameAsc;
                    EnabledSortType = ExternalServiceSortType.EnabledDesc;
                    break;
                case ExternalServiceSortType.NameAsc:
                default:
                    PagedEntities =
                        await _adminServices.PageExternalServicesAsync(
                            TenantId, 
                            (int)(pageNumber ?? 1), 
                            PageSize,
                            ExternalServiceSortType.NameAsc);
                    NameSortType = ExternalServiceSortType.NameDesc;
                    EnabledSortType = ExternalServiceSortType.EnabledDesc;

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
