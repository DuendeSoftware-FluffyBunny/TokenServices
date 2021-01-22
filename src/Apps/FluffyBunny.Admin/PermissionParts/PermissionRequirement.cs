using System;
using Microsoft.AspNetCore.Authorization;

namespace FluffyBunny.Admin.PermissionParts
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string permissionName)
        {
            RequiredPermissions = permissionName ?? throw new ArgumentNullException(nameof(permissionName));
        }

        public string RequiredPermissions { get; }
    }
}