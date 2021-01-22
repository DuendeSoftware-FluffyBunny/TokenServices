using System;
using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using FluffyBunny.Admin.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FluffyBunny.Admin.PermissionParts
{
    public class
        FluffyBunnyAdminPermissionsClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
    {
        private IExternalLoginContext _externalLoginContext;
        private IHttpContextAccessor _httpContextAccessor;

        private string GuidN => Guid.NewGuid().ToString("N");

        public FluffyBunnyAdminPermissionsClaimsPrincipalFactory(
            IExternalLoginContext externalLoginContext,
            IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
            _externalLoginContext = externalLoginContext;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
        {
            var sessionKey = GuidN;
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(".sessionKey", sessionKey));
            _httpContextAccessor.HttpContext.Session.SetString(sessionKey, sessionKey);

            // TODO: Lookup user in our database to 
            if (_externalLoginContext.ExternalLoginInfo.LoginProvider == "google")
            {
                var permissions = (Int64)Permissions.Admin;
                identity.AddClaim(new Claim("permissions", permissions.ToString()));
            }
            else
            {
                var permissions =  (Int64)Permissions.SelfHelp;
                identity.AddClaim(new Claim("permissions", permissions.ToString()));
            }

            return identity;
        }
    }
}