using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Entities;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenant
{
    public class DeleteExternalServiceModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private ILogger<DeleteExternalServiceModel> _logger;

        public DeleteExternalServiceModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            ILogger<DeleteExternalServiceModel> logger)
        {
            _adminServices = adminServices;
            _sessionTenantAccessor = sessionTenantAccessor;
            _logger = logger;
        }
        [BindProperty]
        public string TenantId { get; set; }
        public ExternalService Entity { get; private set; }
        public class InputModel
        {
            public int Id { get; set; }
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public async Task OnGetAsync(int id)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            Entity = await _adminServices.GetExternalServiceByIdAsync(TenantId, id);
            Input = new InputModel()
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
                    await _adminServices.DeleteExternalServiceByIdAsync(TenantId, Input.Id);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }

            return RedirectToPage("./ManageExternalServices");
        }
    }
}
