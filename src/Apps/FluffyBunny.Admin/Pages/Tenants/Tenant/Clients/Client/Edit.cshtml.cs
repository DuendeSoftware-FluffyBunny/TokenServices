using System;
using System.Threading.Tasks;
using FluffyBunny.Admin.Pages.Tenants.Tenant.Clients.Models;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Entities;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.Clients.Client
{
    public class EditModel : PageModel
    {
        private static string GuidS => Guid.NewGuid().ToString();
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private IInputModelMapperAccessor _inputModelMapperAccessor;
        private ILogger<EditModel> _logger;

        public EditModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            IInputModelMapperAccessor inputModelMapperAccessor,
            ILogger<EditModel> logger)
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
            Input = _inputModelMapperAccessor.MapperIgnoreBase.Map<ClientExtraInputModel>(entity);
            Input.Id = id;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {

                var entity = _inputModelMapperAccessor.MapperIgnoreBase.Map<ClientExtra>(Input);
                await _adminServices.UpsertClientAsync(TenantId, entity);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            return RedirectToPage("./Index",new  {id = Input.Id});
        }
    }
}
