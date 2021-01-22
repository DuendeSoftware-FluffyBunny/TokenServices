using System;
using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FluffyBunny.Admin.PermissionParts
{
    public class FluffyBunnyAdminPermissionsClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
    {

        private IHttpContextAccessor _httpContextAccessor;

        private string GuidN => Guid.NewGuid().ToString("N");

        public FluffyBunnyAdminPermissionsClaimsPrincipalFactory(
            IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
        {
            var sessionKey = GuidN;
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(".sessionKey", sessionKey));
            _httpContextAccessor.HttpContext.Session.SetString(sessionKey, sessionKey);

            // TODO: Lookup user in our database to 
            if (string.Compare(user.Email,"BOB@BOB.COM",null,CompareOptions.IgnoreCase) == 0)
            {
                var permissions = (Int64)Permissions.Admin | (Int64)Permissions.SelfHelp;
                identity.AddClaim(new Claim("permissions", permissions.ToString()));
            }

            return identity;
        }
    }
}