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

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.OpenIdConnectAuthorities
{
    public class IndexModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private IPagingHelper _pagingHelper;
        private ILogger _logger;

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
        public OpenIdConnectAuthoritiesSortType NameSortType { get; set; }
        public PaginatedList<OpenIdConnectAuthority> PagedEntities { get; private set; }
        [ViewData]
        public OpenIdConnectAuthoritiesSortType EnabledSortType { get; set; }
        [ViewData]
        public OpenIdConnectAuthoritiesSortType CurrentSortType { get; set; }

        [ViewData]
        public int PageSize { get; set; }

        [BindProperty]
        public int SelectedPageSize { get; set; }
        public async Task OnGetAsync(OpenIdConnectAuthoritiesSortType sortOrder, int? pageNumber, int? pageSize)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            PageSize = _pagingHelper.ValidatePageSize(pageSize);
            PageSizeOptions = _pagingHelper.GetPagingSizeOptions();
            SelectedPageSize = PageSize;

            switch (sortOrder)
            {

                case OpenIdConnectAuthoritiesSortType.EnabledAsc:
                    PagedEntities =
                        await _adminServices.PageOpenIdConnectAuthoritiesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            OpenIdConnectAuthoritiesSortType.EnabledAsc);
                    EnabledSortType = OpenIdConnectAuthoritiesSortType.EnabledDesc;
                    break;
                case OpenIdConnectAuthoritiesSortType.EnabledDesc:
                    PagedEntities =
                        await _adminServices.PageOpenIdConnectAuthoritiesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            OpenIdConnectAuthoritiesSortType.EnabledDesc);
                    EnabledSortType = OpenIdConnectAuthoritiesSortType.EnabledAsc;
                    break;
                case OpenIdConnectAuthoritiesSortType.NameDesc:
                    PagedEntities =
                        await _adminServices.PageOpenIdConnectAuthoritiesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            OpenIdConnectAuthoritiesSortType.NameDesc);
                    NameSortType = OpenIdConnectAuthoritiesSortType.NameAsc;
                    EnabledSortType = OpenIdConnectAuthoritiesSortType.EnabledDesc;
                    break;
                case OpenIdConnectAuthoritiesSortType.NameAsc:
                default:
                    PagedEntities =
                        await _adminServices.PageOpenIdConnectAuthoritiesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            OpenIdConnectAuthoritiesSortType.NameAsc);
                    NameSortType = OpenIdConnectAuthoritiesSortType.NameDesc;
                    EnabledSortType = OpenIdConnectAuthoritiesSortType.EnabledDesc;

                    break;
            }

            CurrentSortType = sortOrder;
        }
        
    }
}
