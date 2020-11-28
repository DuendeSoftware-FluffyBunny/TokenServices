using System.Threading.Tasks;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Entities;

namespace Microsoft.EntityFrameworkCore
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
    }
}