using System;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.ApiResources.Secrets
{
    public class DeleteModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private ILogger<DeleteModel> _logger;

        public DeleteModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            ILogger<DeleteModel> logger)
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


        public async Task OnGetAsync(int apiResourceId, int id)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            ApiResourceId = apiResourceId;
            SecretId = id;
            Secret = await _adminServices.GetApiResourceSecretByIdAsync(TenantId, ApiResourceId, SecretId);
        }
        public async Task<IActionResult> OnPostAsync(string submit)
        {
            try
            {
                if (string.Compare(submit, "delete", true) == 0)
                {
                    await _adminServices.DeleteApiResourceBySecretIdAsync(TenantId,ApiResourceId,SecretId);
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
