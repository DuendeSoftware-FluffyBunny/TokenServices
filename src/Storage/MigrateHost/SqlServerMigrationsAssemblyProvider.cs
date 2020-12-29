using FluffyBunny.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace MigrateHost
{
    public class SqlServerMigrationsAssemblyProvider : IMigrationsAssemblyProvider
    {
        public string AssemblyName => typeof(SqlServer.Anchor).Assembly.FullName;
    }
}