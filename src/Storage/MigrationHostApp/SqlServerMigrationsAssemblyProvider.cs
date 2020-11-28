using Microsoft.EntityFrameworkCore;

namespace MigrationHostApp
{
    public class SqlServerMigrationsAssemblyProvider : IMigrationsAssemblyProvider
    {
        public string AssemblyName => typeof(SqlServer.Anchor).Assembly.FullName;
    }
}