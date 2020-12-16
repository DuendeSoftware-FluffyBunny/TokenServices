using System;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Entities;

namespace Microsoft.EntityFrameworkCore
{
    public interface ITenantAwareConfigurationDbContext : IDisposable
    {
        DbSet<ExternalService> ExternalServices { get; set; }
 
        public DbSet<ClientExtra> Clients { get; set; }
        public DbSet<PersistedGrantExtra> PersistedGrants { get; set; }

        public DbSet<ClientCorsOrigin> ClientCorsOrigins { get; set; }

        public DbSet<IdentityResource> IdentityResources { get; set; }

        public DbSet<ApiResource> ApiResources { get; set; }

        public DbSet<ApiScope> ApiScopes { get; set; }

        
        DbContext DbContext { get; }
        Task<int> SaveChangesAsync();

    }
}