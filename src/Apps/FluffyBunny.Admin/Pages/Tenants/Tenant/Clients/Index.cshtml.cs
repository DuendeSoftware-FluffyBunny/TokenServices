using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.Admin.Services;
using FluffyBunny.EntityFramework.Entities;
using FluffyBunny.IdentityServer.EntityFramework.Storage;
 
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.Clients
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
        public ClientsSortType NameSortType { get; set; }
        public PaginatedList<ClientExtra> PagedEntities { get; private set; }
        [ViewData]
        public ClientsSortType EnabledSortType { get; set; }
        [ViewData]
        public ClientsSortType CurrentSortType { get; set; }

        [ViewData]
        public int PageSize { get; set; }

        [BindProperty]
        public int SelectedPageSize { get; set; }

        public async Task OnGetAsync(ClientsSortType sortOrder, int? pageNumber, int? pageSize)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            PageSize = _pagingHelper.ValidatePageSize(pageSize);
            PageSizeOptions = _pagingHelper.GetPagingSizeOptions();
            SelectedPageSize = PageSize;

            switch (sortOrder)
            {

                case ClientsSortType.EnabledAsc:
                    PagedEntities =
                        await _adminServices.PageClientsAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            ClientsSortType.EnabledAsc);
                    EnabledSortType = ClientsSortType.EnabledDesc;
                    break;
                case ClientsSortType.EnabledDesc:
                    PagedEntities =
                        await _adminServices.PageClientsAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            ClientsSortType.EnabledDesc);
                    EnabledSortType = ClientsSortType.EnabledAsc;
                    break;
                case ClientsSortType.NameDesc:
                    PagedEntities =
                        await _adminServices.PageClientsAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            ClientsSortType.NameDesc);
                    NameSortType = ClientsSortType.NameAsc;
                    EnabledSortType = ClientsSortType.EnabledDesc;
                    break;
                case ClientsSortType.NameAsc:
                default:
                    PagedEntities =
                        await _adminServices.PageClientsAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            ClientsSortType.NameAsc);
                    NameSortType = ClientsSortType.NameDesc;
                    EnabledSortType = ClientsSortType.EnabledDesc;

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
