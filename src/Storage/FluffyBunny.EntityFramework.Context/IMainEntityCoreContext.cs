using System.Threading.Tasks;
using FluffyBunny.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace FluffyBunny.EntityFramework.Context
{
    public interface IMainEntityCoreContext
    {
        DbSet<Tenant> Tenants { get; set; }
        DbContext DbContext { get; }
        Task<int> SaveChangesAsync();
    }
}