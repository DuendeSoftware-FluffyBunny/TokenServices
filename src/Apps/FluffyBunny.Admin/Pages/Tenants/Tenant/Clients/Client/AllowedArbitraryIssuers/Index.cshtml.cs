using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.Admin.Model;
using FluffyBunny.Admin.Services;
using FluffyBunny.EntityFramework.Entities;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.Clients.Client.AllowedArbitraryIssuers
{
    public class IndexModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private IdentityServerDefaultOptions _options;
        private ILogger _logger;
        private ITenantAwareConfigurationDbContextAccessor _tenantAwareConfigurationDbContextAccessor;

        public IndexModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            IOptions<IdentityServerDefaultOptions> options,
            ITenantAwareConfigurationDbContextAccessor tenantAwareConfigurationDbContextAccessor,
            ILogger<IndexModel> logger)
        {
            _adminServices = adminServices;
            _sessionTenantAccessor = sessionTenantAccessor;
            _options = options.Value;
            _tenantAwareConfigurationDbContextAccessor = tenantAwareConfigurationDbContextAccessor;
            _logger = logger;
        }
        [BindProperty]
        public string TenantId { get; set; }
        [BindProperty]
        public int ClientId { get; set; }


        public IEnumerable<EntityFramework.Entities.AllowedArbitraryIssuer> Entities { get; private set; }

        [ViewData]
        public ClientAllowedArbitraryIssuersSortType NameSortType { get; private set; }
        [ViewData]
        public ClientAllowedArbitraryIssuersSortType CurrentSortType { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id, ClientAllowedArbitraryIssuersSortType sortOrder)
        {
            try
            {
                TenantId = _sessionTenantAccessor.TenantId;
                ClientId = id;

                switch (sortOrder)
                {

                    case ClientAllowedArbitraryIssuersSortType.NameAsc:
                        Entities =
                            await _adminServices.GetAllClientAllowedArbitraryIssuersAsync(
                                TenantId,
                                id,
                                ClientAllowedArbitraryIssuersSortType.NameAsc);
                        NameSortType = ClientAllowedArbitraryIssuersSortType.NameDesc;
                        break;
                    case ClientAllowedArbitraryIssuersSortType.NameDesc:
                        Entities =
                            await _adminServices.GetAllClientAllowedArbitraryIssuersAsync(
                                TenantId,
                                id,
                                ClientAllowedArbitraryIssuersSortType.NameDesc);
                        NameSortType = ClientAllowedArbitraryIssuersSortType.NameAsc;
                        break;

                }

                CurrentSortType = sortOrder;
                return Page();

            }
            catch (Exception ex)
            {
                return RedirectToPage("../Index", new
                {
                    id = ClientId
                });
            }
        }
    }
}
