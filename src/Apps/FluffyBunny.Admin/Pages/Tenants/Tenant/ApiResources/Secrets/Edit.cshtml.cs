using System;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using FluffyBunny.Admin.Model;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.ApiResources.Secrets
{
    public class EditModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private ILogger<EditModel> _logger;

        public EditModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            ILogger<EditModel> logger)
        {
            _adminServices = adminServices;
            _sessionTenantAccessor = sessionTenantAccessor;
            _logger = logger;
        }

        [BindProperty]
        public string TenantId { get; set; }
        
        [BindProperty]
        public int ApiResourceId { get; set; }
        
        [BindProperty] 
        public int SecretId { get; set; }
        
        public ApiResourceSecret Secret { get; set; }

        [BindProperty]
        public SecretModel Input { get; set; }
        public async Task OnGetAsync(int apiResourceId, int id)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            ApiResourceId = apiResourceId;
            SecretId = id;
            Secret = await _adminServices.GetApiResourceSecretByIdAsync(TenantId, ApiResourceId, SecretId);

            Input = new SecretModel
            {
               CurrentExpiration = Secret.Expiration,
               SecretExpiration = SecretModel.ExpirationTypes.DoNotChange
            };
        }
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                bool update = true;
                DateTime expiration = DateTime.UtcNow;
                switch (Input.SecretExpiration)
                {
                    case SecretModel.ExpirationTypes.DoNotChange:
                        update = false;
                        break;
                    case SecretModel.ExpirationTypes.Never:
                        expiration = expiration.AddYears(100);
                        break;
                    case SecretModel.ExpirationTypes.ExpireIt:
                        expiration = expiration.AddYears(-100);
                        break;
                    case SecretModel.ExpirationTypes.OneYear:
                        expiration = expiration.AddYears(1);
                        break;
                }

                if (update)
                {
                    Secret = await _adminServices.GetApiResourceSecretByIdAsync(TenantId, ApiResourceId, SecretId);
                    Secret.Expiration = expiration;


                    await _adminServices.UpsertApiResourceSecretAsync(TenantId, ApiResourceId, Secret);
                }
             

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
