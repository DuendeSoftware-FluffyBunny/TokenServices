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

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.Clients.Client.AllowedArbitraryIssuers.AllowedArbitraryIssuer
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
        [BindProperty]
        public int ClientId { get; set; }

        public class InputModel
        {
            [Required]
            [Url]
            public string Issuer { get; set; }
            [Required]
            public int Id { get; set; }
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public async Task OnGetAsync(int clientId, int id)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            ClientId = clientId;
            var existing =
                await _adminServices.GetClientAllowedArbitraryIssuerByIdAsync(TenantId, ClientId, id);
            Input = new InputModel()
            {
                Id = id,
                Issuer = existing.Issuer
            };
        }
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await _adminServices.DeleteClientAllowedArbitraryIssuerByIdAsync(TenantId, ClientId, Input.Id);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }

            return RedirectToPage("../Index", new { id = ClientId });
        }
    }
}
