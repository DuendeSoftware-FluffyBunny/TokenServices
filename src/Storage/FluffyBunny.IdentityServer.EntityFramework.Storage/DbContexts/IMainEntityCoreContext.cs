using System.Threading.Tasks;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Entities;

namespace Microsoft.EntityFrameworkCore
{
    public interface IMainEntityCoreContext
    {
        DbSet<Tenant> Tenants { get; set; }

        DbContext DbContext { get; }
        Task<int> SaveChangesAsync();
    }
}