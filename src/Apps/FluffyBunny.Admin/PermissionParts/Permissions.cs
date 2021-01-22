using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FluffyBunny.Admin.PermissionParts
{
    [Flags]
    public enum Permissions : Int64
    {
        None = 0,
        Admin = 1 << 0,
        SelfHelp = 1 << 1 
    }
}
