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

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.Clients.Client.AllowedTokenExchangeExternalServices
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
        public class AllowedTokenExchangeExternalServiceContainer
        {
            public bool Enabled { get; set; }
            public AllowedTokenExchangeExternalService AllowedTokenExchangeExternalService { get; set; }
        }
        [BindProperty]
        public List<AllowedTokenExchangeExternalServiceContainer> AllowedTokenExchangeExternalServiceContainers { get; set; }
        public async Task<IActionResult> OnGetAsync(int id, ClientAllowedRevokeTokenTypeHintsSortType sortOrder)
        {
            try
            {
                TenantId = _sessionTenantAccessor.TenantId;
                ClientId = id;
                var knownExternalServices =
                    await _adminServices.GetAllExternalServicesAsync(TenantId);

                var entities =
                    await _adminServices.GetAllClientAllowedTokenExchangeExternalServicesAsync(TenantId, id,ClientAllowedTokenExchangeExternalServicesSortType.NameAsc);

                var allowed = (from item in entities
                    select item.ExternalService).ToList();
                var containers = (from item in knownExternalServices
                                  let c = new AllowedTokenExchangeExternalServiceContainer()
                    {
                        Enabled = false,
                        AllowedTokenExchangeExternalService = new AllowedTokenExchangeExternalService() { ExternalService = item.Name }
                    }
                    select c).ToList();
                foreach (var item in containers)
                {
                    item.Enabled = allowed.Contains(item.AllowedTokenExchangeExternalService.ExternalService);
                }

                AllowedTokenExchangeExternalServiceContainers = containers;
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
        public async Task<IActionResult> OnPostAsync()
        {
            TenantId = _sessionTenantAccessor.TenantId;
            var context = _tenantAwareConfigurationDbContextAccessor.GetTenantAwareConfigurationDbContext(TenantId);

            var query = from item in context.Clients
                where item.Id == ClientId
                select item;
            var clientInDB = await query
                .Include(x => x.AllowedTokenExchangeExternalServices)
                .FirstOrDefaultAsync();

            foreach (var item in AllowedTokenExchangeExternalServiceContainers)
            {

                var exitingEntity = clientInDB.AllowedTokenExchangeExternalServices.FirstOrDefault(e => e.ExternalService == item.AllowedTokenExchangeExternalService.ExternalService);
                if (exitingEntity != null)
                {
                    if (!item.Enabled)
                    {
                        // remove it.
                        clientInDB.AllowedTokenExchangeExternalServices.Remove(exitingEntity);
                    }
                }
                else
                {
                    if (item.Enabled)
                    {
                        // add it.
                        clientInDB.AllowedTokenExchangeExternalServices.Add(item.AllowedTokenExchangeExternalService);
                    }
                }
            }
            await context.SaveChangesAsync();
            return RedirectToPage("../Index", new { id = ClientId });
        }
    }
}
