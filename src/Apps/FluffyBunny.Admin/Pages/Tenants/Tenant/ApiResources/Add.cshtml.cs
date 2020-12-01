using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.ApiResources
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
            [Required]
            public bool Enabled { get; set; }
            [Required]
            public string Name { get; set; }  // service name
            [Required]
            public string Description { get; set; }
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
                var entity = new ApiResource()
                {
                    Name = Input.Name,
                    Description = Input.Description,
                    
                    Enabled = Input.Enabled
                };
                await _adminServices.UpsertApiResourceAsync(TenantId, entity);
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
