using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FluffyBunny.Admin.Services;
using FluffyBunny.EntityFramework.Entities;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.ExternalServices
{
    public class AddModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private ILogger<AddModel> _logger;

        public AddModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            ILogger<AddModel> logger)
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
                var entity = new ExternalService
                {
                    Name = Input.Name,
                    Description = Input.Description,
                    Authority = Input.Authority,
                    Enabled = Input.Enabled
                };
                await _adminServices.UpsertExternalServiceAsync(TenantId, entity);
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
