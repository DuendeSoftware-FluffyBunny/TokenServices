using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.Admin.Services;
using FluffyBunny.EntityFramework.Entities;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.OpenIdConnectAuthorities
{
    public class AddModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private ILogger _logger;

        public AddModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            ILogger<ExternalServices.AddModel> logger)
        {
            _adminServices = adminServices;
            _sessionTenantAccessor = sessionTenantAccessor;
            _logger = logger;
        }
        [BindProperty]
        public string TenantId { get; set; }
        public class InputModel
        {
            public int Id { get; set; }
            public string Name { get; set; }  // service name
            [Required]
            public string Description { get; set; }
            [Required]
            public string Authority { get; set; }
            [Required]
            public bool Enabled { get; set; }
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public async Task OnGetAsync()
        {
            TenantId = _sessionTenantAccessor.TenantId;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var entity = new OpenIdConnectAuthority()
                {
                    Name = Input.Name,
                    Description = Input.Description,
                    Authority = Input.Authority,
                    Enabled = Input.Enabled
                };
                await _adminServices.UpsertOpenIdConnectAuthorityAsync(TenantId, entity);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            return RedirectToPage("./Index");
        }
    }
}
