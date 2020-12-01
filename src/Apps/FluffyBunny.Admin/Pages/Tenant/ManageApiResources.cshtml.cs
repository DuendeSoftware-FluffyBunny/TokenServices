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
    public class ManageApiResourcesModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private IPagingHelper _pagingHelper;
        private ILogger<ManageApiResourcesModel> _logger;

        public ManageApiResourcesModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            IPagingHelper pagingHelper,
            ILogger<ManageApiResourcesModel> logger)
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
        public ApiResourcesSortType NameSortType { get; set; }
        public PaginatedList<ApiResource> PagedEntities { get; private set; }
        [ViewData]
        public ApiResourcesSortType EnabledSortType { get; set; }
        [ViewData]
        public ApiResourcesSortType CurrentSortType { get; set; }

        [ViewData]
        public int PageSize { get; set; }

        [BindProperty]
        public int SelectedPageSize { get; set; }

        public async Task OnGetAsync(ApiResourcesSortType sortOrder, int? pageNumber, int? pageSize)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            PageSize = _pagingHelper.ValidatePageSize(pageSize);
            PageSizeOptions = _pagingHelper.GetPagingSizeOptions();
            SelectedPageSize = PageSize;

            switch (sortOrder)
            {

                case ApiResourcesSortType.EnabledAsc:
                    PagedEntities =
                        await _adminServices.PageApiResourcesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            ApiResourcesSortType.EnabledAsc);
                    EnabledSortType = ApiResourcesSortType.EnabledDesc;
                    break;
                case ApiResourcesSortType.EnabledDesc:
                    PagedEntities =
                        await _adminServices.PageApiResourcesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            ApiResourcesSortType.EnabledDesc);
                    EnabledSortType = ApiResourcesSortType.EnabledAsc;
                    break;
                case ApiResourcesSortType.NameDesc:
                    PagedEntities =
                        await _adminServices.PageApiResourcesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            ApiResourcesSortType.NameDesc);
                    NameSortType = ApiResourcesSortType.NameAsc;
                    EnabledSortType = ApiResourcesSortType.EnabledDesc;
                    break;
                case ApiResourcesSortType.NameAsc:
                default:
                    PagedEntities =
                        await _adminServices.PageApiResourcesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            ApiResourcesSortType.NameAsc);
                    NameSortType = ApiResourcesSortType.NameDesc;
                    EnabledSortType = ApiResourcesSortType.EnabledDesc;

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
