using System;
using FluffyBunny.IdentityServer.EntityFramework.Storage.DbContexts;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryDbContextOptionsProvider : IDbContextOptionsProvider
    {
        string GuidS => Guid.NewGuid().ToString(); 
        private string DatabaseName { get; }
        public InMemoryDbContextOptionsProvider()
        {
            DatabaseName = GuidS;
        }
        public void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(DatabaseName);
        }

        public void OnConfiguring(string tenantId, DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase($"{tenantId}-InMemoryDatabase");
        }
    }
}
 
