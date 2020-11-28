
using System;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Extensions;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.EntityFramework.Options;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Entities;

namespace Microsoft.EntityFrameworkCore
{
    public class TenantAwareConfigurationDbContext : DbContext, ITenantAwareConfigurationDbContext 
    {
        private string _tenantId;
        private IDbContextOptionsProvider _dbContextOptionsProvider;
        private ConfigurationStoreOptions _storeOptions;

        // only used to build out migrations
        public TenantAwareConfigurationDbContext(
            DbContextOptions<TenantAwareConfigurationDbContext> options, ConfigurationStoreOptions storeOptions) :
            base(options)
        {
            this._storeOptions = storeOptions ?? throw new ArgumentNullException(nameof(storeOptions));
        }

        public TenantAwareConfigurationDbContext(
            string tenantId, 
            ConfigurationStoreOptions storeOptions,
            IDbContextOptionsProvider dbContextOptionsProvider)
        {
            _tenantId = tenantId;
            _storeOptions = storeOptions;
            _dbContextOptionsProvider = dbContextOptionsProvider;
            _storeOptions.ConfigureDbContext = o =>
            {
                _dbContextOptionsProvider.Configure(o);
            };
        }

        public DbContext DbContext => this;
        /// <summary>
        /// Gets or sets the ExternalServices.
        /// </summary>
        /// <value>
        /// The clients.
        /// </value>
        public DbSet<ExternalService> ExternalServices { get; set; }
 
        /// <summary>
        /// Gets or sets the clients.
        /// </summary>
        /// <value>
        /// The clients.
        /// </value>
        public DbSet<ClientExtra> Clients { get; set; }

        /// <summary>
        /// Gets or sets the clients' CORS origins.
        /// </summary>
        /// <value>
        /// The clients CORS origins.
        /// </value>
        public DbSet<ClientCorsOrigin> ClientCorsOrigins { get; set; }

        /// <summary>
        /// Gets or sets the identity resources.
        /// </summary>
        /// <value>
        /// The identity resources.
        /// </value>
        public DbSet<IdentityResource> IdentityResources { get; set; }

        /// <summary>
        /// Gets or sets the API resources.
        /// </summary>
        /// <value>
        /// The API resources.
        /// </value>
        public DbSet<ApiResource> ApiResources { get; set; }

        /// <summary>
        /// Gets or sets the API scopes.
        /// </summary>
        /// <value>
        /// The API resources.
        /// </value>
        public DbSet<ApiScope> ApiScopes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_dbContextOptionsProvider == null || string.IsNullOrWhiteSpace(_tenantId))
            {
                base.OnConfiguring(optionsBuilder);
            }
            else
            {
                _dbContextOptionsProvider.OnConfiguring(_tenantId, optionsBuilder);
            }
        }
        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }  
        /// Override this method to further configure the model that was discovered by convention from the entity types
        /// exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
        /// and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and other extensions) typically
        /// define extension methods on this object that allow you to configure aspects of the model that are specific
        /// to a given database.</param>
        /// <remarks>
        /// If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
        /// then this method will not be run.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientExtra>()
                .HasIndex(x=>x.ClientId).IsUnique();
            
            modelBuilder.Entity<Tenant>()
                .HasIndex(x => x.Name).IsUnique();

            modelBuilder.Entity<ExternalService>()
                .HasIndex(x => x.Name).IsUnique();

            
            modelBuilder.ConfigureClientContext(_storeOptions);
            modelBuilder.ConfigureResourcesContext(_storeOptions);

            base.OnModelCreating(modelBuilder);
        }
    }
}
 