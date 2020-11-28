using FluffyBunny.IdentityServer.EntityFramework.Storage.DbContexts;
using Microsoft.Extensions.Options;

namespace Microsoft.EntityFrameworkCore
{
    public class PostgresDbContextOptionsProvider : IDbContextOptionsProvider
    {
        private EntityFrameworkConnectionOptions _options;
        public PostgresDbContextOptionsProvider(IOptions<EntityFrameworkConnectionOptions> options)
        {
            _options = options.Value;
        }

        public void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _options.ConnectionString;
            optionsBuilder.UseNpgsql(connectionString);
        }

        public void OnConfiguring(string tenantId, DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _options.ConnectionStringDatabaseTemplate
                                           .Replace("{{Database}}", $"{tenantId}-database");
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}
