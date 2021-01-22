using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace FluffyBunny.Admin.PermissionParts
{
    public static class PermissionExtensions
    {
        /// <summary>
        /// This returns true if the current user has the permission
        /// </summary>
        /// <param name="user"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static bool UserHasThisPermission(this ClaimsPrincipal user, Permissions permission)
        {
            var permissionClaim =
                user?.Claims.SingleOrDefault(x => x.Type == PermissionConstants.PermissionsClaimType);
            if (permissionClaim == null) return false;
            var allowedUserPermissionsFlags = Convert.ToInt64(permissionClaim.Value);
            var requiredUserPermissionsFlags = Convert.ToInt64(permission);
            var result = (allowedUserPermissionsFlags & requiredUserPermissionsFlags) == requiredUserPermissionsFlags;
            return result;
        }

        
    }
}