using System;
using System.Collections.Generic;
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
                var existingEntity = await _adminServices.GetApiResourceByNameAsync(TenantId, Input.Name);
                if (existingEntity != null)
                {
                    ModelState.AddModelError(string.Empty,$"ApiResource {Input.Name} already exists, please pick another name.");
                    return Page();
                }
                var entity = new Duende.IdentityServer.EntityFramework.Entities.ApiResource()
                {
                    Name = Input.Name,
                    Description = Input.Description,
                    Enabled = Input.Enabled,
                    Scopes = new List<ApiResourceScope>()
                    {
                        new ApiResourceScope()
                        {
                            Scope = Input.Name
                        }
                    }
                };
                await _adminServices.UpsertApiResourceAsync(TenantId, entity);
                var entityInDb = await _adminServices.GetApiResourceByNameAsync(TenantId, Input.Name);

                await _adminServices.UpsertApiResourceScopeAsync(TenantId, entityInDb.Id, new ApiResourceScope()
                {
                    Scope = Input.Name
                }); 
                return RedirectToPage("./ApiResource/Index", new { id = entityInDb.Id});
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
           
        }
    }
}
