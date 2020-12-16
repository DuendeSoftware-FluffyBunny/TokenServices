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

namespace FluffyBunny.Admin.Pages.Tenant
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private ILogger<IndexModel> _logger;
        [BindProperty]
        public string TenantId { get; set; }
        public IdentityServer.EntityFramework.Storage.Entities.Tenant Tenant { get; private set; }

        public class InputModel
        {
            public bool Enabled { get; set; }
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public IEnumerable<FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.Tenant> Tenants { get; private set; }

        public IndexModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            ILogger<IndexModel> logger)
        {
            _adminServices = adminServices;
            _sessionTenantAccessor = sessionTenantAccessor;
            _logger = logger;
        }
        public async Task OnGetAsync( )
        {
           TenantId = _sessionTenantAccessor.TenantId;
           Tenant = await _adminServices.GetTenantByNameAsync(TenantId);
           Input = new InputModel()
           {
               Enabled = Tenant.Enabled
           };
        }
        public async Task<IActionResult> OnPostAsync()
        {
            await _adminServices.EnsureTenantDatabaseAsync(TenantId);
            Tenant = await _adminServices.GetTenantByNameAsync(TenantId);
            Tenant.Enabled = Input.Enabled;
            await _adminServices.UpdateTenantAsync(Tenant);
            return RedirectToPage();
        }
    }
}
