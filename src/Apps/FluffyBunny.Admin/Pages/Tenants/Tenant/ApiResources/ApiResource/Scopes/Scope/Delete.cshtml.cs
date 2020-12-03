using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.ApiResources.ApiResource.Scopes.Scope
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

        public class InputModel
        {
            public int Id { get; set; }
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

        public async Task<IActionResult> OnGetAsync(int apiResourceId, int id)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            ApiResourceId = apiResourceId;


            Entity = await _adminServices.GetApiResourceByIdAsync(TenantId, ApiResourceId);
            var scopeEntity = Entity.Scopes.FirstOrDefault(x => x.Id == id);
            if (scopeEntity == null)
            {
                return RedirectToPage("../Index", new { id = ApiResourceId });
            }

            var subScope = scopeEntity.Scope.Substring(Entity.Name.Length + 1);

            Input = new InputModel()
            {
                Id = id,
                PrependName = Entity.Name,
                Scope = subScope
            };
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string submit)
        {
            try
            {
                if (string.Compare(submit, "delete", true) == 0)
                {
                    await _adminServices.DeleteApiResourceByScopeIdAsync(TenantId, ApiResourceId, Input.Id);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }

            return RedirectToPage("../Index", new { id = ApiResourceId });
        }
    }
}
