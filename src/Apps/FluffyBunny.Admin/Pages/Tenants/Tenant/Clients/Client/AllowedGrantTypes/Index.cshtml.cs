using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using FluffyBunny.Admin.Model;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.Clients.AllowedGrantTypes
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
        public List<GrantTypeContainer> GrantTypeContainers { get; set; }
 

        public class GrantTypeContainer
        {
            public bool Enabled { get; set; }
            public ClientGrantType ClientGrantType { get; set; }
        }
        public class ClientGrantTypeComparer : EqualityComparer<ClientGrantType>
        {
            public override bool Equals(ClientGrantType x, ClientGrantType y)
            {
                return x.GrantType == y.GrantType;
            }

            public override int GetHashCode([DisallowNull] ClientGrantType obj)
            {
                return obj.GrantType.GetHashCode();
            }
        }
        public async Task OnGetAsync(int id)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            ClientId = id;
            var entities = (
                await _adminServices.GetAllClientAllowedGrantTypesAsync(
                    TenantId,
                    ClientId,
                    GrantTypesSortType.NameAsc)).ToList();
            var allowedGrantTypes = (from item in entities
                                     select item.GrantType).ToList();

            var containers = (from item in _options.AvailableGrantTypes
                let c = new GrantTypeContainer()
                {
                    Enabled = false,
                    ClientGrantType = new ClientGrantType() {GrantType = item}
                }
                select c).ToList();

            foreach (var item in containers)
            {
                item.Enabled = allowedGrantTypes.Contains(item.ClientGrantType.GrantType);
            }

            GrantTypeContainers = containers;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            TenantId = _sessionTenantAccessor.TenantId;
            var context = _tenantAwareConfigurationDbContextAccessor.GetTenantAwareConfigurationDbContext(TenantId);

            var query = from item in context.Clients
                select item;
            var clientInDB = await query
                .Include(x => x.AllowedGrantTypes)
                .FirstOrDefaultAsync();

            foreach (var item in GrantTypeContainers)
            {

                var exitingEntity = clientInDB.AllowedGrantTypes.FirstOrDefault(e => e.GrantType == item.ClientGrantType.GrantType);
                if (exitingEntity != null)
                {
                    if (!item.Enabled)
                    {
                        // remove it.
                        clientInDB.AllowedGrantTypes.Remove(exitingEntity);
                    }
                }
                else
                {
                    if (item.Enabled)
                    {
                        // add it.
                        clientInDB.AllowedGrantTypes.Add(item.ClientGrantType);
                    }
                }
            }
            await context.SaveChangesAsync();
            return RedirectToPage("../Index",new {id=ClientId});
        }

    }
}
