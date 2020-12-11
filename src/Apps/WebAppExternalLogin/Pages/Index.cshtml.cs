using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebAppExternalLogin.Pages
{
    public class IndexModel : PageModel
    {
        public List<Claim> Claims { get; set; }
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            if (User.Identity.IsAuthenticated)
            {

                Claims = Request.HttpContext.User.Claims.ToList();
            }
        }
    }
}
