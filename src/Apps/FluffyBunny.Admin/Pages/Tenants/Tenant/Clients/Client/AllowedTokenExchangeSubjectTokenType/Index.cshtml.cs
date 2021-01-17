using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.Admin.Model;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.Clients.Client.AllowedTokenExchangeSubjectTokenType
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
        public class AllowedTokenExchangeSubjectTokenTypeContainer
        {
            public bool Enabled { get; set; }
            public EntityFramework.Entities.AllowedTokenExchangeSubjectTokenType AllowedTokenExchangeSubjectTokenType { get; set; }
        }
        [BindProperty]
        public List<AllowedTokenExchangeSubjectTokenTypeContainer> AllowedTokenExchangeSubjectTokenTypeContainers { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, ClientAllowedRevokeTokenTypeHintsSortType sortOrder)
        {
            try
            {
                TenantId = _sessionTenantAccessor.TenantId;
                ClientId = id;
                
                

                var entities =
                    await _adminServices.GetAllClientAllowedTokenExchangeSubjectTokenTypesAsync(TenantId, id, ClientAllowedTokenExchangeSubjectTokenTypesSortType.NameAsc);

                var allowed = (from item in entities
                    select item.SubjectTokenType).ToList();
                var containers = (from item in _options.AvailableSubjectTokenTypes
                                  let c = new AllowedTokenExchangeSubjectTokenTypeContainer()
                    {
                        Enabled = false,
                        AllowedTokenExchangeSubjectTokenType = new EntityFramework.Entities.AllowedTokenExchangeSubjectTokenType() { SubjectTokenType = item }
                    }
                    select c).ToList();
                foreach (var item in containers)
                {
                    item.Enabled = allowed.Contains(item.AllowedTokenExchangeSubjectTokenType.SubjectTokenType);
                }

                AllowedTokenExchangeSubjectTokenTypeContainers = containers;
               
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
                .Include(x => x.AllowedTokenExchangeSubjectTokenTypes)
                .FirstOrDefaultAsync();

            foreach (var item in AllowedTokenExchangeSubjectTokenTypeContainers)
            {

                var exitingEntity = clientInDB.AllowedTokenExchangeSubjectTokenTypes.FirstOrDefault(e => e.SubjectTokenType == item.AllowedTokenExchangeSubjectTokenType.SubjectTokenType);
                if (exitingEntity != null)
                {
                    if (!item.Enabled)
                    {
                        // remove it.
                        clientInDB.AllowedTokenExchangeSubjectTokenTypes.Remove(exitingEntity);
                    }
                }
                else
                {
                    if (item.Enabled)
                    {
                        // add it.
                        clientInDB.AllowedTokenExchangeSubjectTokenTypes.Add(item.AllowedTokenExchangeSubjectTokenType);
                    }
                }
            }
            await context.SaveChangesAsync();
            return RedirectToPage("../Index", new { id = ClientId });
        }
    }
}
