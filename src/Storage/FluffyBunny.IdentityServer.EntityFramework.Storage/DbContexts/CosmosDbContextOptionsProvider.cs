using Microsoft.Extensions.Options;

namespace Microsoft.EntityFrameworkCore
{
    public class CosmosDbContextOptionsProvider : IDbContextOptionsProvider
    {
        private CosmosDbConfiguration _options;

        public CosmosDbContextOptionsProvider(IOptions<CosmosDbConfiguration> options)
        {
            _options = options.Value;
        }

        public void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseCosmos(_options.EndPointUrl, _options.PrimaryKey, _options.DatabaseName);
            optionsBuilder.UseLazyLoadingProxies();
        }

        public void OnConfiguring(string tenantId, DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseCosmos(
                _options.EndPointUrl,
                _options.PrimaryKey,
                $"{tenantId}-{_options.DatabaseName}");
            optionsBuilder.UseLazyLoadingProxies();
        }
    }
}