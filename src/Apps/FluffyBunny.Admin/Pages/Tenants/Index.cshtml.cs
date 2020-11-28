using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.Admin.Services;
 
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private ILogger<AddTenantModel> _logger;

        public string TenantId { get; private set; }

        public class InputModel
        {
            [Required]
            public string Name { get; set; }
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public IEnumerable<FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.Tenant> Tenants { get; private set; }

        public IndexModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            ILogger<AddTenantModel> logger)
        {
            _adminServices = adminServices;
            _sessionTenantAccessor = sessionTenantAccessor;
            _logger = logger;
        }
        public async Task OnGetAsync()
        {
            Tenants = await _adminServices.GetAllTenantsAsync();
            TenantId = _sessionTenantAccessor.TenantId;
        }
        public async Task<IActionResult> OnPostAsync(string tenantName)
        {
            _sessionTenantAccessor.TenantId = tenantName;
            return RedirectToPage("/Tenant/Index");
        }
    }
}
