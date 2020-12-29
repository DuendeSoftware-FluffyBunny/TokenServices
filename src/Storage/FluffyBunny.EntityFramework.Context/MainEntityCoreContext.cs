using System.Threading.Tasks;
using FluffyBunny.EntityFramework.Context.Extensions;
using FluffyBunny.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace FluffyBunny.EntityFramework.Context
{
    public class MainEntityCoreContext : DbContext, IMainEntityCoreContext
    {

        public MainEntityCoreContext(DbContextOptions<MainEntityCoreContext> options) : base(options) { }
        public DbSet<Tenant> Tenants { get; set; }

        public DbContext DbContext => this;

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureTenantContext();
        }
    }
}