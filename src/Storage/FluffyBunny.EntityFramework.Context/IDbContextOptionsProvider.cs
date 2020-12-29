using Microsoft.EntityFrameworkCore;

namespace FluffyBunny.EntityFramework.Context
{
    public interface IDbContextOptionsProvider
    {
        void Configure(DbContextOptionsBuilder optionsBuilder);
        void OnConfiguring(string tenantId, DbContextOptionsBuilder optionsBuilder);
    }
}