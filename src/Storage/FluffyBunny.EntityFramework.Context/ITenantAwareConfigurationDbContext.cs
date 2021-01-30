using System;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using FluffyBunny.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace FluffyBunny.EntityFramework.Context
{
    public interface ITenantAwareConfigurationDbContext : IDisposable
    {
        public DbSet<OpenIdConnectAuthority> OpenIdConnectAuthorities { get; set; }
        public DbSet<ExternalService> ExternalServices { get; set; }

        public DbSet<ClientExtra> Clients { get; set; }
        public DbSet<PersistedGrantExtra> PersistedGrants { get; set; }
        public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }

        public DbSet<ClientCorsOrigin> ClientCorsOrigins { get; set; }

        public DbSet<IdentityResource> IdentityResources { get; set; }

        public DbSet<ApiResource> ApiResources { get; set; }

        public DbSet<ApiScope> ApiScopes { get; set; }

        public DbSet<SelfHelpUser> SelfHelpUsers { get; set; }
        public DbSet<AllowedSelfHelpClient> AllowedClients { get; set; }

        DbContext DbContext { get; }
        Task<int> SaveChangesAsync();

    }
}