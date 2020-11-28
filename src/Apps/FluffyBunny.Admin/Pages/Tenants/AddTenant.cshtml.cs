using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Logging;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants
{
    [Authorize]
    public class AddTenantModel : PageModel
    {
        private IAdminServices _adminServices;
        private ILogger<AddTenantModel> _logger;

        public class InputModel
        {
            [Required]
            public string Name { get; set; }
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public AddTenantModel(
            IAdminServices adminServices,
            ILogger<AddTenantModel> logger)
        {
            _adminServices = adminServices;
            _logger = logger;
        }
        public async Task OnGetAsync()
        {
        }
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await _adminServices.CreateTenantAsync(Input.Name);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Failed Create Tenant={Input.Name}",ex.Message,ex);
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            _logger.LogInformation($"Created Tenant={Input.Name}");
            return RedirectToPage("./Index");
        }
    }
}
