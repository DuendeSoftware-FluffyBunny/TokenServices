using System;

namespace FluffyBunny.Admin.PermissionParts
{
    public static class PermissionChecker
    {
        /// <summary>
        /// This is used by the policy provider to check the permission name string
        /// </summary>
        /// <param name="packedPermissions"></param>
        /// <param name="permissionName"></param>
        /// <returns></returns>
        public static bool ThisPermissionIsAllowed(this string allowedUserPermissions, string requiredUserPermissions)
        {
            var allowedUserPermissionsFlags = Convert.ToInt64(allowedUserPermissions);
            var requiredUserPermissionsFlags = Convert.ToInt64((Permissions)Enum.Parse(typeof(Permissions), requiredUserPermissions));
            var result = (allowedUserPermissionsFlags & requiredUserPermissionsFlags) == requiredUserPermissionsFlags;
            return result;
        }
    }
}