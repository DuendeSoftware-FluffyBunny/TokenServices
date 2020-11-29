using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.Admin.Services;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin.Pages.Tenant
{
    public class ManageApiResourcesModel : PageModel
    {
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private IPagingHelper _pagingHelper;
        private ILogger<ManageApiResourcesModel> _logger;

        public ManageApiResourcesModel(
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            IPagingHelper pagingHelper,
            ILogger<ManageApiResourcesModel> logger)
        {
            _adminServices = adminServices;
            _sessionTenantAccessor = sessionTenantAccessor;
            _pagingHelper = pagingHelper;
            _logger = logger;
        }

        public string TenantId { get; set; }

        public async Task OnGetAsync()
        {
            TenantId = _sessionTenantAccessor.TenantId;

        }
    }
}