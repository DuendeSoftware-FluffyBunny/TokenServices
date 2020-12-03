using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.ApiResources.ApiResource.Scopes
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

        public class InputModel
        {
            [Required]
            public string PrependName { get; set; }
            [Required]
            public string Scope { get; set; }
        }
        [BindProperty]
        public InputModel Input { get; set; }
        [BindProperty]
        public string TenantId { get; set; }
        [BindProperty]
        public int ApiResourceId { get; set; }
        public Duende.IdentityServer.EntityFramework.Entities.ApiResource Entity { get; private set; }

        public async Task OnGetAsync(int id)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            ApiResourceId = id;
            Entity = await _adminServices.GetApiResourceByIdAsync(TenantId, ApiResourceId);
            Input = new InputModel()
            {
                PrependName = Entity.Name
            };
        }
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var fullScopeName = $"{Input.PrependName}.{Input.Scope}";
                var existingScope =
                    await _adminServices.GetApiResourceScopeByNameAsync(TenantId, ApiResourceId, fullScopeName);
                if (existingScope != null)
                {

                    ModelState.AddModelError(string.Empty, $"{fullScopeName} already exists");
                    return Page();
                }

                await _adminServices.UpsertApiResourceScopeAsync(TenantId, ApiResourceId, new ApiResourceScope()
                {
                    Scope = fullScopeName
                });

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }

            return RedirectToPage("./Index", new { id = ApiResourceId });
        }
    }
}
