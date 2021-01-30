using System;
using System.Collections.Generic;
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
    public class DeleteModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private ILogger _logger;

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
        public OpenIdConnectAuthority Entity { get; private set; }
        public class InputModel
        {
            public int Id { get; set; }
        }
        [BindProperty]
        public ExternalServices.DeleteModel.InputModel Input { get; set; }
        public async Task OnGetAsync(int id)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            Entity = await _adminServices.GetOpenIdConnectAuthorityByIdAsync(TenantId, id);
            Input = new ExternalServices.DeleteModel.InputModel()
            {
                Id = Entity.Id,
            };
        }
        public async Task<IActionResult> OnPostAsync(string submit)
        {
            try
            {
                if (string.Compare(submit, "delete", true) == 0)
                {
                    await _adminServices.DeleteOpenIdConnectAuthorityByIdAsync(TenantId, Input.Id);
                }
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

