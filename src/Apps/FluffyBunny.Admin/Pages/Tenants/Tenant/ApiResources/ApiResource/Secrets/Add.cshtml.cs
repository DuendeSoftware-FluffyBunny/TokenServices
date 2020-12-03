using System;
using System.Threading.Tasks;
using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.Models;
using FluffyBunny.Admin.Model;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using FluffyBunny4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.ApiResources.ApiResource.Secrets
{
    public class AddModel : PageModel
    {
        private IAdminServices _adminServices;
        private IPasswordGenerator _passwordGenerator;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private ILogger<AddModel> _logger;

        public AddModel(
            IAdminServices adminServices,
            IPasswordGenerator passwordGenerator,
            ISessionTenantAccessor sessionTenantAccessor,
            ILogger<AddModel> logger)
        {
            _adminServices = adminServices;
            _passwordGenerator = passwordGenerator;
            _sessionTenantAccessor = sessionTenantAccessor;
            _logger = logger;
        }

        [BindProperty]
        public string TenantId { get; set; }
        [BindProperty]
        public int ApiResourceId { get; set; }
       

 
        [BindProperty]
        public SecretModel Input { get; set; }
        public async Task OnGetAsync(int id)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            ApiResourceId = id;
           

            Input = new SecretModel
            {
                Value = _passwordGenerator.GeneratePassword(),
                SecretExpiration = "Never"
            };
        }
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                DateTime expiration = DateTime.UtcNow;
                switch (Input.SecretExpiration)
                {
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

                var secret = new ApiResourceSecret()
                {
                    Type = IdentityServerConstants.SecretTypes.SharedSecret,
                    Expiration = expiration,
                    Description = Input.Value.Mask(4, '*'),
                    Value = Input.Value.Sha256()
                };

 
                await _adminServices.UpsertApiResourceSecretAsync(TenantId,ApiResourceId, secret);

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
