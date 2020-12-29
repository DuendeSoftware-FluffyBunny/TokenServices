using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using FluffyBunny.Admin.Pages.Tenants.Tenant.Clients.Models;
using FluffyBunny.Admin.Services;
using FluffyBunny.EntityFramework.Entities;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.Clients
{
    public class AddModel : PageModel
    {
        private static string GuidS => Guid.NewGuid().ToString();
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private IInputModelMapperAccessor _inputModelMapperAccessor;
        private ILogger<AddModel> _logger;

        public AddModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            IInputModelMapperAccessor inputModelMapperAccessor,
            ILogger<AddModel> logger)
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

        public async Task OnGetAsync()
        {
            TenantId = _sessionTenantAccessor.TenantId;
            var clientId = GuidS;
            Input = _inputModelMapperAccessor.MapperIgnoreBase.Map<ClientExtraInputModel>(new ClientExtra()
            {
                ClientId = clientId,
                Description = clientId,
                ClientName = clientId,
                AllowOfflineAccess = true,
                IncludeClientId = true,
                AccessTokenType = (int)AccessTokenType.Reference
                
            });
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
            return RedirectToPage("./Index");
        }
    }
}
