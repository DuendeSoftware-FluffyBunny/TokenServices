using System;
using System.Globalization;
using System.Linq;
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
            var claimsPrincipal = _externalLoginContext.ExternalLoginInfo.Principal;
            var sessionKey = GuidN;
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(".sessionKey", sessionKey));
            _httpContextAccessor.HttpContext.Session.SetString(sessionKey, sessionKey);

            // TODO: Lookup user in our database to 
            Int64 permissions = 0;
            Claim preferredUsernameClaim;
            switch (_externalLoginContext.ExternalLoginInfo.LoginProvider)
            {
                case "azuread-artificer":
                    preferredUsernameClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "preferred_username");
                    identity.AddClaim(new Claim("preferred_username", $"{preferredUsernameClaim.Value}"));
                    if (claimsPrincipal.HasClaim(ClaimTypes.Role, "Task.Administrator"))
                    {
                        permissions = permissions | (Int64) Permissions.Admin;
                    }
                    if (claimsPrincipal.HasClaim(ClaimTypes.Role, "Task.SelfHelp"))
                    {
                        permissions = permissions | (Int64)Permissions.SelfHelp;
                    }
                    
                    break;
                case "google":
                    permissions = (Int64)Permissions.Admin;
                    preferredUsernameClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "name");
                    identity.AddClaim(new Claim("preferred_username", $"{preferredUsernameClaim.Value} (google)"));
                    break;
                default:
                    preferredUsernameClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "name");
                    identity.AddClaim(new Claim("preferred_username", $"{preferredUsernameClaim.Value} (demo)"));
                    permissions = (Int64)Permissions.SelfHelp;
                    break;
            }
            identity.AddClaim(new Claim("permissions", permissions.ToString()));

            return identity;
        }

       
    }
}