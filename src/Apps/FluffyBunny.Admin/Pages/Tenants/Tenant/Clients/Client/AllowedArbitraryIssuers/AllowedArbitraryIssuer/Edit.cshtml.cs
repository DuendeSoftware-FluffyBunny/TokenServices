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
    public class EditModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private ILogger _logger;

        public EditModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            ILogger<AddModel> logger)
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
                var existing =
                    await _adminServices.GetClientAllowedArbitraryIssuerByNameAsync(TenantId, ClientId, Input.Issuer);
                if (existing != null)
                {

                    ModelState.AddModelError(string.Empty, $"{Input.Issuer} already exists");
                    return Page();
                }

                
                await _adminServices.UpsertClientAllowedArbitraryIssuerAsync(TenantId, ClientId, new EntityFramework.Entities.AllowedArbitraryIssuer()
                {
                    Id = Input.Id,
                    Issuer = Input.Issuer
                });

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
