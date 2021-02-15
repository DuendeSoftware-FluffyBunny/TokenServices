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

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.Certificates
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

       
        public PaginatedList<Certificate> PagedEntities { get; private set; }

        [ViewData]
        public CertificatesSortType SigningAlgorithmSortType { get; set; }
        [ViewData]
        public CertificatesSortType NotBeforeSortType { get; set; }
        [ViewData]
        public CertificatesSortType ExpirationSortType { get; set; }
        [ViewData]
        public CertificatesSortType CurrentSortType { get; set; }

        [ViewData]
        public int PageSize { get; set; }

        [BindProperty]
        public int SelectedPageSize { get; set; }
        public async Task OnGetAsync(CertificatesSortType sortOrder, int? pageNumber, int? pageSize)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            PageSize = _pagingHelper.ValidatePageSize(pageSize);
            PageSizeOptions = _pagingHelper.GetPagingSizeOptions();
            SelectedPageSize = PageSize;

            switch (sortOrder)
            {
                case CertificatesSortType.SigningAlgorithmAsc:
                    PagedEntities =
                        await _adminServices.PageCertificatesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            CertificatesSortType.ExpirationAsc);
                    SigningAlgorithmSortType = CertificatesSortType.SigningAlgorithmDesc;
                    NotBeforeSortType = CertificatesSortType.NotBeforeDesc;
                    ExpirationSortType = CertificatesSortType.ExpirationDesc;
                    break;
                case CertificatesSortType.SigningAlgorithmDesc:
                    PagedEntities =
                        await _adminServices.PageCertificatesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            CertificatesSortType.ExpirationDesc);
                    SigningAlgorithmSortType = CertificatesSortType.SigningAlgorithmAsc;
                    NotBeforeSortType = CertificatesSortType.NotBeforeDesc;
                    ExpirationSortType = CertificatesSortType.ExpirationDesc;
                    break;
                case CertificatesSortType.ExpirationAsc:
                    PagedEntities =
                        await _adminServices.PageCertificatesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            CertificatesSortType.ExpirationAsc);
                    SigningAlgorithmSortType = CertificatesSortType.SigningAlgorithmDesc;
                    NotBeforeSortType = CertificatesSortType.NotBeforeDesc;
                    ExpirationSortType = CertificatesSortType.ExpirationDesc;
                    break;
                case CertificatesSortType.ExpirationDesc:
                    PagedEntities =
                        await _adminServices.PageCertificatesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            CertificatesSortType.ExpirationDesc);
                    SigningAlgorithmSortType = CertificatesSortType.SigningAlgorithmDesc;
                    NotBeforeSortType = CertificatesSortType.NotBeforeDesc;
                    ExpirationSortType = CertificatesSortType.ExpirationAsc;
                    break;
                case CertificatesSortType.NotBeforeAsc:
                    PagedEntities =
                        await _adminServices.PageCertificatesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            CertificatesSortType.NotBeforeAsc);
                    SigningAlgorithmSortType = CertificatesSortType.SigningAlgorithmDesc;
                    NotBeforeSortType = CertificatesSortType.NotBeforeDesc;
                    ExpirationSortType = CertificatesSortType.ExpirationDesc;
                    break;
                case CertificatesSortType.NotBeforeDesc:
                default:
                    PagedEntities =
                        await _adminServices.PageCertificatesAsync(
                            TenantId,
                            (int)(pageNumber ?? 1),
                            PageSize,
                            CertificatesSortType.NotBeforeDesc);
                    SigningAlgorithmSortType = CertificatesSortType.SigningAlgorithmDesc;
                    NotBeforeSortType = CertificatesSortType.NotBeforeAsc;
                    ExpirationSortType = CertificatesSortType.ExpirationDesc;

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
