using System;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Extensions;
using Duende.IdentityServer.EntityFramework.Options;
using FluffyBunny.EntityFramework.Context.Extensions;
using FluffyBunny.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace FluffyBunny.EntityFramework.Context
{
    public class TenantAwareConfigurationDbContext : DbContext, ITenantAwareConfigurationDbContext
    {
        private string _tenantId;
        private IDbContextOptionsProvider _dbContextOptionsProvider;
        private ConfigurationStoreOptions _storeOptions;
        private OperationalStoreOptions _operationalStoreOptions;

        public TenantAwareConfigurationDbContext(ConfigurationStoreOptions storeOptions, OperationalStoreOptions operationalStoreOptions, DbContextOptions<TenantAwareConfigurationDbContext> options)
            : base(options)
        {
            this._storeOptions = storeOptions ?? throw new ArgumentNullException(nameof(storeOptions));
            this._operationalStoreOptions = operationalStoreOptions ?? throw new ArgumentNullException(nameof(operationalStoreOptions));
        }
        public TenantAwareConfigurationDbContext(
            string tenantId,
            ConfigurationStoreOptions storeOptions,
            OperationalStoreOptions operationalStoreOptions,
            IDbContextOptionsProvider dbContextOptionsProvider)
        {

            this._storeOptions = storeOptions ?? throw new ArgumentNullException(nameof(storeOptions));
            this._operationalStoreOptions = operationalStoreOptions ?? throw new ArgumentNullException(nameof(operationalStoreOptions));
            _tenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
            _dbContextOptionsProvider = dbContextOptionsProvider ?? throw new ArgumentNullException(nameof(dbContextOptionsProvider));

        }
        public DbContext DbContext => this;
        public DbSet<ClientExtra> Clients { get; set; }
        public DbSet<ExternalService> ExternalServices { get; set; }
        public DbSet<PersistedGrantExtra> PersistedGrants { get; set; }
        public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }
        public DbSet<ClientCorsOrigin> ClientCorsOrigins { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }
        public DbSet<ApiScope> ApiScopes { get; set; }
        public DbSet<AllowedArbitraryIssuer> AllowedArbitraryIssuers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (string.IsNullOrWhiteSpace(_tenantId))
            {
                base.OnConfiguring(optionsBuilder);
            }
            else
            {
                _dbContextOptionsProvider.OnConfiguring(_tenantId, optionsBuilder);

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureAllowedArbitraryIssuerContext();
            modelBuilder.ConfigureExternalServicesContext();
            modelBuilder.ConfigureClientContext(_storeOptions);
            modelBuilder.ConfigureResourcesContext(_storeOptions);
            modelBuilder.ConfigurePersistedGrantContext(_operationalStoreOptions);

            modelBuilder.Entity<ClientExtra>(client =>
            {
                client.HasMany(x => x.AllowedArbitraryIssuers)
                    .WithOne(x => x.Client)
                    .HasForeignKey(x => x.ClientId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            });

        }
        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }
    }
}