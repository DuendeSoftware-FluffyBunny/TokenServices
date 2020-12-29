using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FluffyBunny.Admin.Services
{
    public class SqlServerMigrationsAssemblyProvider : IMigrationsAssemblyProvider
    {
        public string AssemblyName => typeof(SqlServer.Anchor).Assembly.FullName;
    }
}
