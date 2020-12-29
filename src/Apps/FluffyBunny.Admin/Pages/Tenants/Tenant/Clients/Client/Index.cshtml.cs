using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.Admin.Pages.Tenants.Tenant.Clients.Models;
using FluffyBunny.Admin.Services;
using FluffyBunny.EntityFramework.Entities;
 
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.Clients.Client
{
    public class IndexModel : PageModel
    {
        private static string GuidS => Guid.NewGuid().ToString();
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private IInputModelMapperAccessor _inputModelMapperAccessor;
        private ILogger<IndexModel> _logger;

        public IndexModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            IInputModelMapperAccessor inputModelMapperAccessor,
            ILogger<IndexModel> logger)
        {
            _adminServices = adminServices;
            _sessionTenantAccessor = sessionTenantAccessor;
            _inputModelMapperAccessor = inputModelMapperAccessor;
            _logger = logger;
        }
        [BindProperty]
        public string TenantId { get; set; }
        public ClientExtra Entity { get; set; }
       

        public async Task OnGetAsync(int id)
        {
            TenantId = _sessionTenantAccessor.TenantId;
            Entity = await _adminServices.GetClientByIdAsync(TenantId, id);
 
        }
    }
}
