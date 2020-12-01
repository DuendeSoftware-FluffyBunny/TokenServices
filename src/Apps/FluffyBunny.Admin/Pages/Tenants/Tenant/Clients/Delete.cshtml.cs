using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.Admin.Pages.Tenants.Tenant.Clients.Models;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Entities;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.Clients
{
    public class DeleteModel : PageModel
    {
        private static string GuidS => Guid.NewGuid().ToString();
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private IInputModelMapperAccessor _inputModelMapperAccessor;
        private ILogger<DeleteModel> _logger;

        public DeleteModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            IInputModelMapperAccessor inputModelMapperAccessor,
            ILogger<DeleteModel> logger)
        {
            _adminServices = adminServices;
            _sessionTenantAccessor = sessionTenantAccessor;
            _inputModelMapperAccessor = inputModelMapperAccessor;
            _logger = logger;
        }
        [BindProperty]
        public string TenantId { get; set; }

        [BindProperty]
        public ClientExtraInputModel Input { get; set; }
      

        public async Task OnGetAsync(int id)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            var entity = await _adminServices.GetClientByIdAsync(TenantId, id);
            Input = _inputModelMapperAccessor.MapperOneToOne.Map<ClientExtraInputModel>(entity);
        }
        public async Task<IActionResult> OnPostAsync(string submit)
        {
            try
            {
                if (string.Compare(submit, "delete", true) == 0)
                {
                    await _adminServices.DeleteClientByIdAsync(TenantId, Input.Id);
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
