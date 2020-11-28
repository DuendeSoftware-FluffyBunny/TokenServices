using Microsoft.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore
{
    public interface IDbContextOptionsProvider
    {
        void Configure(DbContextOptionsBuilder optionsBuilder);
        void OnConfiguring(string tenantId, DbContextOptionsBuilder optionsBuilder);
    }
}