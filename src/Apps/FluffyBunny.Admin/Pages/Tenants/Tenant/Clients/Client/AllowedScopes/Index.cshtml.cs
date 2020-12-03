using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using FluffyBunny.Admin.Model;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.Clients.Client.AllowedScopes
{
    public class IndexModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private IdentityServerDefaultOptions _options;
        private ILogger<IndexModel> _logger;
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

        [BindProperty]
        public List<ApiResourceScopeContainer> ApiResourceScopeContainers { get; set; }


        public class ApiResourceScopeContainer
        {
            public bool Enabled { get; set; }
            public ApiResourceScope ApiResourceScope { get; set; }
        }

        public async Task OnGetAsync(int id)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            ClientId = id;
            var availableScopes = (
                await _adminServices.GetAllApiResourceScopesAsync(
                    TenantId,
                    ClientScopesSortType.NameAsc)).ToList();
          
            var client = await _adminServices.GetClientByIdAsync(TenantId, ClientId);
            var enabledScopeNames = (from item in client.AllowedScopes
                                     select item.Scope).ToList();


            var containers = (from item in availableScopes
                              let c = new ApiResourceScopeContainer()
                {
                    Enabled = false,
                    ApiResourceScope = item
                }
                select c).ToList();

            foreach (var item in containers)
            {
                item.Enabled = enabledScopeNames.Contains(item.ApiResourceScope.Scope);
            }

            ApiResourceScopeContainers = containers;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            TenantId = _sessionTenantAccessor.TenantId;
            var context = _tenantAwareConfigurationDbContextAccessor.GetTenantAwareConfigurationDbContext(TenantId);

            var query = from item in context.Clients
                where item.Id == ClientId
                select item;
            var clientInDB = await query
                .Include(x => x.AllowedScopes)
                .FirstOrDefaultAsync();

            foreach (var item in ApiResourceScopeContainers)
            {

                var exitingEntity = clientInDB.AllowedScopes.FirstOrDefault(e => e.Scope == item.ApiResourceScope.Scope);
                if (exitingEntity != null)
                {
                    if (!item.Enabled)
                    {
                        // remove it.
                        clientInDB.AllowedScopes.Remove(exitingEntity);
                    }
                }
                else
                {
                    if (item.Enabled)
                    {
                        // add it.
                        clientInDB.AllowedScopes.Add(new ClientScope()
                        {
                            Scope = item.ApiResourceScope.Scope
                        });
                    }
                }
            }
            await context.SaveChangesAsync();
            return RedirectToPage("../Index", new { id = ClientId });
        }
    }
}
